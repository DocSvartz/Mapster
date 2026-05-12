using BenchmarkDotNet.Attributes;

namespace Mapster.Benchmark.Benchmarks
{
    public abstract class MappingBenchmarkBase
    {
        public IEnumerable<int> MapOperationValues => new[] { 1_000, 10_000, 100_000, 1_000_000 };

        [ParamsSource(nameof(MapOperationValues))]
        public int MapOperations { get; set; }
    }
}