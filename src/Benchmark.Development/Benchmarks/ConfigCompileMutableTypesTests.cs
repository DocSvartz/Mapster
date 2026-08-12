using BenchmarkDotNet.Attributes;
using Mapster.Benchmark.Development.Classes;

namespace Mapster.Benchmark.Development.Benchmarks
{

    public class ConfigCompileMutableTypesTests
    {
        [Params(10_000, 100_000)]
        public int Iterations { get; set; }

        [Benchmark]
        public void MapsterTest()
        {
            var config = new TypeAdapterConfig();

            config.NewConfig<Foo, Foo>();
            config.NewConfig<Foo, Customer>();
            config.NewConfig<Customer, Foo>();
            config.NewConfig<Customer[], Foo[]>();
            config.NewConfig<Foo[], Customer[]>();
            config.Compile();
        }
    }
}
