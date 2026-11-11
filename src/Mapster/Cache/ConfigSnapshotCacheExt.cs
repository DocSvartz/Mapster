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
        /// Blocks the calling thread until the snapshot becomes ready, i.e. all
        /// its rules finish configuration. The wait starts with a short busy-spin
        /// and switches to a 1 ms sleep once the spin phase is exhausted, so the
        /// thread yields to the OS instead of consuming CPU for long waits.
        /// Throws <see cref="TimeoutException"/> if readiness is not reached
        /// within the specified timeout.
        /// </summary>
        /// <param name="timeout">Maximum time to wait for readiness.</param>
        public void WaitReady(TimeSpan timeout)
        {
            if (IsReady) return;

            var started = Stopwatch.StartNew();
            var spinner = new SpinWait();

            while (true)
            {
                if (IsReady) return;
                if (started.Elapsed > timeout)
                    throw new TimeoutException($"Snapshot {Version} was not ready within {timeout}");

                if (spinner.NextSpinWillYield)
                    Thread.Sleep(1);
                else
                    spinner.SpinOnce();
            }
        }

        /// <summary>
        /// Asynchronously waits until the snapshot becomes ready, i.e. all its
        /// rules finish configuration. The awaiting code is suspended between
        /// checks, so no thread is occupied during the wait.
        /// Supports cancellation via the provided token.
        /// </summary>
        /// <param name="ct">Token to cancel the wait before readiness is reached.</param>
        public async Task WaitReadyAsync(CancellationToken ct = default)
        {
            while (!IsReady)
                await Task.Delay(TimeSpan.FromMilliseconds(1), ct);
        }



    }
}
