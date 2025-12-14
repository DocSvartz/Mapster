using Benchmark.Classes;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Benchmarks
{
    public class TestAll
    {
        private Foo _fooInstance;
        private Customer _customerInstance;

        [Params(100)]//, 1_000_000)]
        public int Iterations { get; set; }

        [Benchmark]
        public void MapsterTest()
        {
            TestAdaptHelper.TestMapsterAdapter<Foo, Foo>(_fooInstance, Iterations);
            TestAdaptHelper.TestMapsterAdapter<Customer, CustomerDTO>(_customerInstance, Iterations);
        }

        [Benchmark]
        public void MapsterLocalConfig()
        {
            TestAdaptHelperLocal.TestMapsterAdapter<Foo, Foo>(_fooInstance, Iterations);
            TestAdaptHelperLocal.TestMapsterAdapter<Customer, CustomerDTO>(_customerInstance, Iterations);
        }


        [GlobalSetup(Target = nameof(MapsterTest))]
        public void SetupMapster()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureMapster(_fooInstance, MapsterCompilerType.Default);
            TestAdaptHelper.ConfigureMapster(_customerInstance, MapsterCompilerType.Default);
        }

        [GlobalSetup(Target = nameof(MapsterLocalConfig))]
        public void SetupMapsterLocal()
        {
            _fooInstance = TestAdaptHelperLocal.SetupFooInstance();
            _customerInstance = TestAdaptHelperLocal.SetupCustomerInstance();
            TestAdaptHelperLocal.ConfigureMapster(_fooInstance, MapsterCompilerType.Default);
            TestAdaptHelperLocal.ConfigureMapster(_customerInstance, MapsterCompilerType.Default);
        }
    }
}