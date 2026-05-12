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
                typeof(TestSimpleTypes),
                typeof(TestComplexTypes),
                typeof(TestAll),
            });

            switcher.Run(args, new Config());
        }
    }
}
