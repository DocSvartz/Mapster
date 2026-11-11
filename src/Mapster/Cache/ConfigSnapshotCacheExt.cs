using Mapster.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Mapster
{
    public partial class TypeAdapterConfigSnapshot
    {
        private readonly ConcurrentDictionary<TypeTuple, Delegate> _mapDict = new ConcurrentDictionary<TypeTuple, Delegate>();
        private readonly ConcurrentDictionary<TypeTuple, Delegate> _mapToTargetDict = new ConcurrentDictionary<TypeTuple, Delegate>();
        private readonly ConcurrentDictionary<TypeTuple, MethodCallExpression> _projectionDict = new ConcurrentDictionary<TypeTuple, MethodCallExpression>();
        private readonly ConcurrentDictionary<TypeTuple, Delegate> _dynamicMapDict = new ConcurrentDictionary<TypeTuple, Delegate>();

        internal Dictionary<Type, IMemberModelEx[]> TypeMemberModelsCache { get; init; }
        internal Dictionary<CompiledTypesSettings, TypeAdapterRule> CompiledRule = new();
        internal ConcurrentDictionary<CompiledTypesSettings,bool> CacheCompiledTypesSettings { get; init; } = new();


        /// <summary>
        /// Indicates whether all rules in this snapshot have completed configuration.
        /// Returns true only when every rule in the map is marked as read-only;
        /// returns false if at least one rule is still being populated.
        /// </summary>
        public bool IsReady
        {
            get
            {
                foreach (var rule in RuleMap.Values)
                    if (!rule.Settings.IsReadOnly) return false;
                return true;
            }
        }

        /// <summary>
        /// Blocks the calling thread until all rules in this snapshot are configured.
        /// Fast path: a bounded busy-spin covers sub-millisecond completion times,
        /// which is typical when producers finish quickly. Only after the spin
        /// budget is exhausted does the thread fall back to sleeping, yielding
        /// the CPU to the OS. Throws <see cref="TimeoutException"/> on timeout.
        /// </summary>
        /// <param name="timeout">Maximum time to wait for readiness.</param>
        /// <param name="spinBudget">
        /// Number of busy-spin iterations before switching to sleep. Increase for
        /// very short-lived configuration, decrease if CPU usage during waits matters.
        /// </param>
        /// <param name="delayMilliseconds">
        /// Sleep interval used after the spin budget is exhausted. Note that the
        /// effective granularity on Windows can be up to ~15 ms regardless of this value.
        /// </param>
        public void WaitReady(TimeSpan timeout, long spinBudget = 1000, int delayMilliseconds = 1)
        {
            if (IsReady) return;

            var started = Stopwatch.StartNew();
            var spinner = new SpinWait();


            while (true)
            {
                if (IsReady) return;
                if (started.Elapsed > timeout)
                    throw new TimeoutException($"Snapshot {Version} was not ready within {timeout}");

                if (spinBudget > 0)
                {
                    spinBudget--;
                    spinner.SpinOnce();
                }
                else
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(delayMilliseconds));
                }
            }
        }

        /// <summary>
        /// Asynchronously waits until all rules in this snapshot are configured.
        /// Uses <see cref="Task.Yield"/> between checks: the continuation is
        /// re-queued on the scheduler after every check, so readiness is detected
        /// within one scheduler turn — no timer-granularity delay. Intended for
        /// short waits where minimal detection latency matters.
        /// </summary>
        /// <param name="ct">Token to cancel the wait before readiness is reached.</param>
        public async Task WaitReadyAsyncYield(CancellationToken ct = default)
        {
            while (!IsReady)
            {
                ct.ThrowIfCancellationRequested();
                await Task.Yield();
            }
        }

        /// <summary>
        /// Asynchronously waits until all rules in this snapshot are configured.
        /// Fast path: a bounded number of <see cref="Task.Yield"/> turns catches
        /// sub-millisecond completions with minimal latency. After the fast-check
        /// budget is spent, the loop switches to <see cref="Task.Delay"/> so the
        /// calling thread is fully released between checks instead of occupying
        /// the scheduler while waiting.
        /// </summary>
        /// <param name="ct">Token to cancel the wait before readiness is reached.</param>
        /// <param name="delayMilliseconds">
        /// Delay between checks in the slow phase. The effective granularity on
        /// Windows can be up to ~15 ms regardless of this value.
        /// </param>
        public async Task WaitReadyAsync(CancellationToken ct = default, int delayMilliseconds = 1)
        {
            int fastChecks = 8; // fast-path budget; tune to the typical completion burst

            while (!IsReady)
            {
                ct.ThrowIfCancellationRequested();

                if (fastChecks > 0)
                {
                    fastChecks--;
                    await Task.Yield();
                }
                else
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(delayMilliseconds), ct);
                }
            }
        }


    }
}
