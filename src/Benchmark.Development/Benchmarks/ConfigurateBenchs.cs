using BenchmarkDotNet.Attributes;
using Mapster.Benchmark.Development.Classes;

namespace Mapster.Benchmark.Development.Benchmarks
{
    public class ConfigurateBenchs
    {
        private Customer _customerInstance = TestAdaptHelper.SetupCustomerInstance();

        [Params(1_000_000)]//, 1_000_000)]
        public int Iterations { get; set; }

        [Benchmark]
        public void Clone()
        {
           var s = TypeAdapterConfig.GlobalSettings.Clone();
        }

        [Benchmark]
        public void ForkWithNewConfig()
        {
            var s = TypeAdapterConfig.GlobalSettings.Fork(cfg => { cfg.NewConfig<Customer, CustomerDTO>(); }, 
                "Test", Random.Shared.Next());
        }

        [Benchmark]
        public void ForkWithForType()
        {
            var s = TypeAdapterConfig.GlobalSettings.Fork(cfg => { cfg.ForType<Customer, CustomerDTO>(); },
                "Test", Random.Shared.Next());
        }

        [Benchmark]
        public void InvalidateCacheAndMap_NotUsingStaticCache()
        {

           TypeAdapterConfig.GlobalSettings.NewConfig<Customer, CustomerDTO>();

            var result = _customerInstance.Adapt<CustomerDTO>();

        }

        [Benchmark]
        public void InvalidateCacheAndMap_UsingStaticCache()
        {
            TypeAdapterConfig.GlobalSettings.NewConfig<Customer, CustomerDTO>().Compile();

            var result = _customerInstance.Adapt<Customer,CustomerDTO>();

        }


        [GlobalSetup(Target = nameof(ConfigurateBenchs))]
        public void SetupMapster()
        {
            TypeAdapterConfig.GlobalSettings.NewConfig<Customer, CustomerDTO>();
        }



    }
}
