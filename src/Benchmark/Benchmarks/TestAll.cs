using BenchmarkDotNet.Attributes;
using Mapster.Benchmark.Classes;

namespace Mapster.Benchmark.Benchmarks
{
    public class TestAll : MappingBenchmarkBase
    {
        private Foo _fooInstance;
        private Customer _customerInstance;

        [Benchmark(Baseline = true, Description = $"Mapster {TestAdaptHelper.MapsterVersion}")]
        public void MapsterTest()
        {
            TestAdaptHelper.TestMapsterAdapter<Foo, Foo>(_fooInstance, MapOperations);
            TestAdaptHelper.TestMapsterAdapter<Customer, CustomerDTO>(_customerInstance, MapOperations);
        }

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Roslyn)")]
        public void RoslynTest()
        {
            TestAdaptHelper.TestMapsterAdapter<Foo, Foo>(_fooInstance, MapOperations);
            TestAdaptHelper.TestMapsterAdapter<Customer, CustomerDTO>(_customerInstance, MapOperations);
        }

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (FEC)")]
        public void FecTest()
        {
            TestAdaptHelper.TestMapsterAdapter<Foo, Foo>(_fooInstance, MapOperations);
            TestAdaptHelper.TestMapsterAdapter<Customer, CustomerDTO>(_customerInstance, MapOperations);
        }

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Codegen)")]
        public void CodegenTest()
        {
            TestAdaptHelper.TestCodeGen(_fooInstance, MapOperations);
            TestAdaptHelper.TestCodeGen(_customerInstance, MapOperations);
        }

        [Benchmark(Description = $"AutoMapper {TestAdaptHelper.AutoMapperVersion}")]
        public void AutoMapperTest()
        {
            TestAdaptHelper.TestAutoMapper<Foo, Foo>(_fooInstance, MapOperations);
            TestAdaptHelper.TestAutoMapper<Customer, CustomerDTO>(_customerInstance, MapOperations);
        }

        [Benchmark(Description = $"Facet {TestAdaptHelper.FacetVersion}")]
        public void FacetTest()
        {
            TestAdaptHelper.TestFacet(_fooInstance, MapOperations);
            TestAdaptHelper.TestFacet(_customerInstance, MapOperations);
        }

        [Benchmark(Description = $"Mapperly {TestAdaptHelper.MapperlyVersion}")]
        public void MapperlyTest()
        {
            TestAdaptHelper.TestMapperly(_fooInstance, MapOperations);
            TestAdaptHelper.TestMapperly(_customerInstance, MapOperations);
        }

        [GlobalSetup(Target = nameof(MapsterTest))]
        public void SetupMapster()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureMapster<Foo, Foo>(_fooInstance, MapsterCompilerType.Default);
            TestAdaptHelper.ConfigureMapster<Customer, CustomerDTO>(_customerInstance, MapsterCompilerType.Default);
        }

        [GlobalSetup(Target = nameof(RoslynTest))]
        public void SetupRoslyn()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureMapster<Foo, Foo>(_fooInstance, MapsterCompilerType.Roslyn);
            TestAdaptHelper.ConfigureMapster<Customer, CustomerDTO>(_customerInstance, MapsterCompilerType.Roslyn);
        }

        [GlobalSetup(Target = nameof(FecTest))]
        public void SetupFec()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureMapster<Foo, Foo>(_fooInstance, MapsterCompilerType.FEC);
            TestAdaptHelper.ConfigureMapster<Customer, CustomerDTO>(_customerInstance, MapsterCompilerType.FEC);
        }

        [GlobalSetup(Target = nameof(CodegenTest))]
        public void SetupCodegen()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            _ = FooMapper.Map(_fooInstance);
            _ = CustomerMapper.Map(_customerInstance);
        }

        [GlobalSetup(Target = nameof(FacetTest))]
        public void SetupFacet()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureFacet(_fooInstance);
            TestAdaptHelper.ConfigureFacet(_customerInstance);
        }

        [GlobalSetup(Target = nameof(MapperlyTest))]
        public void SetupMapperly()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureMapperly(_fooInstance);
            TestAdaptHelper.ConfigureMapperly(_customerInstance);
        }

        [GlobalSetup(Target = nameof(AutoMapperTest))]
        public void SetupAutoMapper()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            _customerInstance = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.ConfigureAutoMapper<Foo, Foo>(_fooInstance);
            TestAdaptHelper.ConfigureAutoMapper<Customer, CustomerDTO>(_customerInstance);
        }
    }
}