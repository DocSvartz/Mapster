using BenchmarkDotNet.Attributes;
using Mapster.Benchmark.Classes;
using Mapster.Benchmark.Comparisons;

namespace Mapster.Benchmark.Benchmarks
{
    // Customer/CustomerDTO: nested object of different type, two collection shape changes
    // (Address[] -> AddressDTO[], ICollection<Address> -> List<AddressDTO>) and a flattening rule (AddressCity <- Address.City).
    public class TestComplexTypes : MappingBenchmarkBase
    {
        private static readonly Func<Customer, CustomerFacetDto> CustomerFacetCompiled =
            CustomerFacetDto.Projection.Compile();

        private Customer _customer;

        [Benchmark(Baseline = true, Description = $"Mapster {TestAdaptHelper.MapsterVersion}")]
        public void MapsterTest()
            => TestAdaptHelper.Loop(_customer, src => src.Adapt<Customer, CustomerDTO>(), MapOperations);

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Roslyn)")]
        public void RoslynTest()
            => TestAdaptHelper.Loop(_customer, src => src.Adapt<Customer, CustomerDTO>(), MapOperations);

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (FEC)")]
        public void FecTest()
            => TestAdaptHelper.Loop(_customer, src => src.Adapt<Customer, CustomerDTO>(), MapOperations);

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Codegen)")]
        public void CodegenTest()
            => TestAdaptHelper.Loop(_customer, CustomerMapper.Map, MapOperations);

        [Benchmark(Description = $"AutoMapper {TestAdaptHelper.AutoMapperVersion}")]
        public void AutoMapperTest()
            => TestAdaptHelper.Loop(_customer, src => TestAdaptHelper.AutoMapper.Map<Customer, CustomerDTO>(src), MapOperations);

        [Benchmark(Description = $"Facet {TestAdaptHelper.FacetVersion}")]
        public void FacetTest()
            => TestAdaptHelper.Loop(_customer, src => new CustomerFacetDto(src), MapOperations);

        [Benchmark(Description = $"Facet {TestAdaptHelper.FacetVersion} (Compiled Projection)")]
        public void FacetCompiledTest()
            => TestAdaptHelper.Loop(_customer, CustomerFacetCompiled, MapOperations);

        [Benchmark(Description = $"Mapperly {TestAdaptHelper.MapperlyVersion}")]
        public void MapperlyTest()
            => TestAdaptHelper.Loop(_customer, MapperlyMappings.MapCustomer, MapOperations);

        [GlobalSetup(Target = nameof(MapsterTest))]
        public void SetupMapster()
        {
            _customer = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.UseMapsterCompiler(MapsterCompilerType.Default);
            _ = _customer.Adapt<Customer, CustomerDTO>();
        }

        [GlobalSetup(Target = nameof(RoslynTest))]
        public void SetupRoslyn()
        {
            _customer = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.UseMapsterCompiler(MapsterCompilerType.Roslyn);
            _ = _customer.Adapt<Customer, CustomerDTO>();
        }

        [GlobalSetup(Target = nameof(FecTest))]
        public void SetupFec()
        {
            _customer = TestAdaptHelper.SetupCustomerInstance();
            TestAdaptHelper.UseMapsterCompiler(MapsterCompilerType.FEC);
            _ = _customer.Adapt<Customer, CustomerDTO>();
        }

        [GlobalSetup(Target = nameof(CodegenTest))]
        public void SetupCodegen() => _customer = TestAdaptHelper.SetupCustomerInstance();

        [GlobalSetup(Target = nameof(AutoMapperTest))]
        public void SetupAutoMapper() => _customer = TestAdaptHelper.SetupCustomerInstance();

        [GlobalSetup(Target = nameof(FacetTest))]
        public void SetupFacet() => _customer = TestAdaptHelper.SetupCustomerInstance();

        [GlobalSetup(Target = nameof(FacetCompiledTest))]
        public void SetupFacetCompiled() => _customer = TestAdaptHelper.SetupCustomerInstance();

        [GlobalSetup(Target = nameof(MapperlyTest))]
        public void SetupMapperly() => _customer = TestAdaptHelper.SetupCustomerInstance();
    }
}
