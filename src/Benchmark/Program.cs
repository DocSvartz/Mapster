using BenchmarkDotNet.Running;
using Mapster.Benchmark.Benchmarks;

namespace Mapster.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            var switcher = new BenchmarkSwitcher(new[]
            {
                typeof(TestFlatTypes),
                typeof(TestRecursiveTypes),
                typeof(TestComplexTypes),
                typeof(TestTotalAllTypes),
            });

            switcher.Run(args, new Config());
        }
    }
}
