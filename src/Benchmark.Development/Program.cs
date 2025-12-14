using Benchmark.Benchmarks;
using BenchmarkDotNet.Running;

var switcher = new BenchmarkSwitcher(new[]
            {
                typeof(TestSimpleTypes),
                typeof(TestComplexTypes),
                typeof(TestAll),
            });

switcher.Run(args, new Config());
