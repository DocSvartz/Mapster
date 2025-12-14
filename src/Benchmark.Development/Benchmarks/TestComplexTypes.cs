using Benchmark.Classes;
using BenchmarkDotNet.Attributes;

namespace Benchmark.Benchmarks
{
    public class TestComplexTypes
    {
        private Customer _customerInstance;

        [Params(1000, 10_000, 100_000, 1_000_000)]
        public int Iterations { get; set; }

        [Benchmark]
        public void MapsterTest()
        {
            TestAdaptHelper.TestMapsterAdapter<Customer, CustomerDTO>(_customerInstance, Iterations);
        }
         
        [GlobalSetup(Target = nameof(MapsterTest))]
        public void SetupMapster()
        {
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureMapster(_customerInstance, MapsterCompilerType.Default);
        }
    }
}