using BenchmarkDotNet.Attributes;
using Mapster.Benchmark.Classes;

namespace Mapster.Benchmark.Benchmarks
{
    public class TestComplexTypes : MappingBenchmarkBase
    {
        private Customer _customerInstance;

        [Benchmark(Baseline = true, Description = $"Mapster {TestAdaptHelper.MapsterVersion}")]
        public void MapsterTest()
        {
            TestAdaptHelper.TestMapsterAdapter<Customer, CustomerDTO>(_customerInstance, MapOperations);
        }

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Roslyn)")]
        public void RoslynTest()
        {
            TestAdaptHelper.TestMapsterAdapter<Customer, CustomerDTO>(_customerInstance, MapOperations);
        }

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (FEC)")]
        public void FecTest()
        {
            TestAdaptHelper.TestMapsterAdapter<Customer, CustomerDTO>(_customerInstance, MapOperations);
        }

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Codegen)")]
        public void CodegenTest()
        {
            TestAdaptHelper.TestCodeGen(_customerInstance, MapOperations);
        }

        [Benchmark(Description = $"AutoMapper {TestAdaptHelper.AutoMapperVersion}")]
        public void AutoMapperTest()
        {
            TestAdaptHelper.TestAutoMapper<Customer, CustomerDTO>(_customerInstance, MapOperations);
        }

        [Benchmark(Description = $"Facet {TestAdaptHelper.FacetVersion}")]
        public void FacetTest()
        {
            TestAdaptHelper.TestFacet(_customerInstance, MapOperations);
        }

        [Benchmark(Description = $"Mapperly {TestAdaptHelper.MapperlyVersion}")]
        public void MapperlyTest()
        {
            TestAdaptHelper.TestMapperly(_customerInstance, MapOperations);
        }

        [GlobalSetup(Target = nameof(MapsterTest))]
        public void SetupMapster()
        {
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureMapster<Customer, CustomerDTO>(_customerInstance, MapsterCompilerType.Default);
        }

        [GlobalSetup(Target = nameof(RoslynTest))]
        public void SetupRoslyn()
        {
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureMapster<Customer, CustomerDTO>(_customerInstance, MapsterCompilerType.Roslyn);
        }

        [GlobalSetup(Target = nameof(FecTest))]
        public void SetupFec()
        {
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureMapster<Customer, CustomerDTO>(_customerInstance, MapsterCompilerType.FEC);
        }

        [GlobalSetup(Target = nameof(CodegenTest))]
        public void SetupCodegen()
        {
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            _ = CustomerMapper.Map(_customerInstance);
        }

        [GlobalSetup(Target = nameof(FacetTest))]
        public void SetupFacet()
        {
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureFacet(_customerInstance);
        }

        [GlobalSetup(Target = nameof(MapperlyTest))]
        public void SetupMapperly()
        {
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureMapperly(_customerInstance);
        }

        [GlobalSetup(Target = nameof(AutoMapperTest))]
        public void SetupAutoMapper()
        {
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureAutoMapper<Customer, CustomerDTO>(_customerInstance);
        }
    }
}