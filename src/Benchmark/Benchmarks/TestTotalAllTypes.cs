using BenchmarkDotNet.Attributes;
using Mapster.Benchmark.Classes;
using Mapster.Benchmark.Comparisons;

namespace Mapster.Benchmark.Benchmarks
{
    /// <summary>
    /// Total benchmark across all three sample shapes:
    /// <list type="bullet">
    ///   <item><see cref="Person"/> -> <see cref="PersonDTO"/> (FlatType DTO)</item>
    ///   <item><see cref="Foo"/> -> <see cref="FooDTO"/> (self-recursive graph)</item>
    ///   <item><see cref="Customer"/> -> <see cref="CustomerDTO"/> (nested + collections + flattening)</item>
    /// </list>
    /// Each [Benchmark] iteration runs all three scenarios via <see cref="MappingBenchmarkBase.MapOperations"/>
    /// loops, so the reported <c>Mean</c> is their total time.
    /// </summary>
    public class TestTotalAllTypes : MappingBenchmarkBase
    {
        private static readonly Func<Foo, FooFacetDto> FooFacetCompiled = FooFacetDto.Projection.Compile();
        private static readonly Func<Customer, CustomerFacetDto> CustomerFacetCompiled = CustomerFacetDto.Projection.Compile();
        private static readonly Func<Person, PersonFacetDto> PersonFacetCompiled = PersonFacetDto.Projection.Compile();

        private Person _person;
        private Foo _foo;
        private Customer _customer;

        [Benchmark(Baseline = true, Description = $"Mapster {TestAdaptHelper.MapsterVersion}")]
        public void MapsterTest()
        {
            TestAdaptHelper.Loop(_person, src => src.Adapt<Person, PersonDTO>(), MapOperations);
            TestAdaptHelper.Loop(_foo, src => src.Adapt<Foo, FooDTO>(), MapOperations);
            TestAdaptHelper.Loop(_customer, src => src.Adapt<Customer, CustomerDTO>(), MapOperations);
        }

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Roslyn)")]
        public void RoslynTest()
        {
            TestAdaptHelper.Loop(_person, src => src.Adapt<Person, PersonDTO>(), MapOperations);
            TestAdaptHelper.Loop(_foo, src => src.Adapt<Foo, FooDTO>(), MapOperations);
            TestAdaptHelper.Loop(_customer, src => src.Adapt<Customer, CustomerDTO>(), MapOperations);
        }

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (FEC)")]
        public void FecTest()
        {
            TestAdaptHelper.Loop(_person, src => src.Adapt<Person, PersonDTO>(), MapOperations);
            TestAdaptHelper.Loop(_foo, src => src.Adapt<Foo, FooDTO>(), MapOperations);
            TestAdaptHelper.Loop(_customer, src => src.Adapt<Customer, CustomerDTO>(), MapOperations);
        }

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Codegen)")]
        public void CodegenTest()
        {
            TestAdaptHelper.Loop(_person, PersonMapper.Map, MapOperations);
            TestAdaptHelper.Loop(_foo, FooMapper.Map, MapOperations);
            TestAdaptHelper.Loop(_customer, CustomerMapper.Map, MapOperations);
        }

        [Benchmark(Description = $"AutoMapper {TestAdaptHelper.AutoMapperVersion}")]
        public void AutoMapperTest()
        {
            TestAdaptHelper.Loop(_person, src => TestAdaptHelper.AutoMapper.Map<Person, PersonDTO>(src), MapOperations);
            TestAdaptHelper.Loop(_foo, src => TestAdaptHelper.AutoMapper.Map<Foo, FooDTO>(src), MapOperations);
            TestAdaptHelper.Loop(_customer, src => TestAdaptHelper.AutoMapper.Map<Customer, CustomerDTO>(src), MapOperations);
        }

        [Benchmark(Description = $"Facet {TestAdaptHelper.FacetVersion}")]
        public void FacetTest()
        {
            TestAdaptHelper.Loop(_person, src => new PersonFacetDto(src), MapOperations);
            TestAdaptHelper.Loop(_foo, src => new FooFacetDto(src), MapOperations);
            TestAdaptHelper.Loop(_customer, src => new CustomerFacetDto(src), MapOperations);
        }

        [Benchmark(Description = $"Facet {TestAdaptHelper.FacetVersion} (Compiled Projection)")]
        public void FacetCompiledTest()
        {
            TestAdaptHelper.Loop(_person, PersonFacetCompiled, MapOperations);
            TestAdaptHelper.Loop(_foo, FooFacetCompiled, MapOperations);
            TestAdaptHelper.Loop(_customer, CustomerFacetCompiled, MapOperations);
        }

        [Benchmark(Description = $"Mapperly {TestAdaptHelper.MapperlyVersion}")]
        public void MapperlyTest()
        {
            TestAdaptHelper.Loop(_person, MapperlyMappings.MapPerson, MapOperations);
            TestAdaptHelper.Loop(_foo, MapperlyMappings.MapFoo, MapOperations);
            TestAdaptHelper.Loop(_customer, MapperlyMappings.MapCustomer, MapOperations);
        }

        [GlobalSetup(Target = nameof(MapsterTest))]
        public void SetupMapster()
        {
            SetupInstances();
            TestAdaptHelper.UseMapsterCompiler(MapsterCompilerType.Default);
            _ = _person.Adapt<Person, PersonDTO>();
            _ = _foo.Adapt<Foo, FooDTO>();
            _ = _customer.Adapt<Customer, CustomerDTO>();
        }

        [GlobalSetup(Target = nameof(RoslynTest))]
        public void SetupRoslyn()
        {
            SetupInstances();
            TestAdaptHelper.UseMapsterCompiler(MapsterCompilerType.Roslyn);
            _ = _person.Adapt<Person, PersonDTO>();
            _ = _foo.Adapt<Foo, FooDTO>();
            _ = _customer.Adapt<Customer, CustomerDTO>();
        }

        [GlobalSetup(Target = nameof(FecTest))]
        public void SetupFec()
        {
            SetupInstances();
            TestAdaptHelper.UseMapsterCompiler(MapsterCompilerType.FEC);
            _ = _person.Adapt<Person, PersonDTO>();
            _ = _foo.Adapt<Foo, FooDTO>();
            _ = _customer.Adapt<Customer, CustomerDTO>();
        }

        [GlobalSetup(Target = nameof(CodegenTest))]
        public void SetupCodegen() => SetupInstances();

        [GlobalSetup(Target = nameof(AutoMapperTest))]
        public void SetupAutoMapper() => SetupInstances();

        [GlobalSetup(Target = nameof(FacetTest))]
        public void SetupFacet() => SetupInstances();

        [GlobalSetup(Target = nameof(FacetCompiledTest))]
        public void SetupFacetCompiled() => SetupInstances();

        [GlobalSetup(Target = nameof(MapperlyTest))]
        public void SetupMapperly() => SetupInstances();

        private void SetupInstances()
        {
            _person = TestAdaptHelper.SetupPersonInstance();
            _foo = TestAdaptHelper.SetupFooInstance();
            _customer = TestAdaptHelper.SetupCustomerInstance();
        }
    }
}