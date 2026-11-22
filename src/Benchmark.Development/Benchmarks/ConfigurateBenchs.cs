using BenchmarkDotNet.Attributes;
using Mapster.Benchmark.Development.Classes;

namespace Mapster.Benchmark.Development.Benchmarks
{
    public class ConfigurateBenchs
    {
        private Foo _fooInstance;
        private Customer _customerInstance;

        [Params(100_000)]//, 1_000_000)]
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

            TypeAdapterConfig.GlobalSettings.NewConfig<Customer, CustomerDTO>().Compile();

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
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();

            TypeAdapterConfig.GlobalSettings.NewConfig<Customer, CustomerDTO>();

           // TestAdaptHelper.ConfigureMapster(_fooInstance, MapsterCompilerType.Default);
           // TestAdaptHelper.ConfigureMapster(_customerInstance, MapsterCompilerType.Default);
        }



    }
}
