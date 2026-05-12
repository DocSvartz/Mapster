using BenchmarkDotNet.Running;
using Mapster.Benchmark.Development.Benchmarks;

var switcher = new BenchmarkSwitcher(new[]
            {
                typeof(TestSimpleTypes),
                typeof(TestComplexTypes),
                typeof(TestAll),
            });

switcher.Run(args, new Config());
