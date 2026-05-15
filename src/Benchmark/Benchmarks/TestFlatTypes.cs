using BenchmarkDotNet.Attributes;
using Mapster.Benchmark.Classes;
using Mapster.Benchmark.Comparisons;

namespace Mapster.Benchmark.Benchmarks
{
    // FlatType DTO: simple property-to-property copy, no nesting, no collections.
    // Highlights pure per-call overhead (delegate dispatch, allocation rate, IL quality of property copy).
    public class TestFlatTypes : MappingBenchmarkBase
    {
        private static readonly Func<Person, PersonFacetDto> PersonFacetCompiled =
            PersonFacetDto.Projection.Compile();

        private Person _person;

        [Benchmark(Baseline = true, Description = $"Mapster {TestAdaptHelper.MapsterVersion}")]
        public void MapsterTest()
            => TestAdaptHelper.Loop(_person, src => src.Adapt<Person, PersonDTO>(), MapOperations);

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Roslyn)")]
        public void RoslynTest()
            => TestAdaptHelper.Loop(_person, src => src.Adapt<Person, PersonDTO>(), MapOperations);

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (FEC)")]
        public void FecTest()
            => TestAdaptHelper.Loop(_person, src => src.Adapt<Person, PersonDTO>(), MapOperations);

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Codegen)")]
        public void CodegenTest()
            => TestAdaptHelper.Loop(_person, PersonMapper.Map, MapOperations);

        [Benchmark(Description = $"AutoMapper {TestAdaptHelper.AutoMapperVersion}")]
        public void AutoMapperTest()
            => TestAdaptHelper.Loop(_person, src => TestAdaptHelper.AutoMapper.Map<Person, PersonDTO>(src), MapOperations);

        [Benchmark(Description = $"Facet {TestAdaptHelper.FacetVersion}")]
        public void FacetTest()
            => TestAdaptHelper.Loop(_person, src => new PersonFacetDto(src), MapOperations);

        [Benchmark(Description = $"Facet {TestAdaptHelper.FacetVersion} (Compiled Projection)")]
        public void FacetCompiledTest()
            => TestAdaptHelper.Loop(_person, PersonFacetCompiled, MapOperations);

        [Benchmark(Description = $"Mapperly {TestAdaptHelper.MapperlyVersion}")]
        public void MapperlyTest()
            => TestAdaptHelper.Loop(_person, MapperlyMappings.MapPerson, MapOperations);

        [GlobalSetup(Target = nameof(MapsterTest))]
        public void SetupMapster()
        {
            _person = TestAdaptHelper.SetupPersonInstance();
            TestAdaptHelper.UseMapsterCompiler(MapsterCompilerType.Default);
            _ = _person.Adapt<Person, PersonDTO>();
        }

        [GlobalSetup(Target = nameof(RoslynTest))]
        public void SetupRoslyn()
        {
            _person = TestAdaptHelper.SetupPersonInstance();
            TestAdaptHelper.UseMapsterCompiler(MapsterCompilerType.Roslyn);
            _ = _person.Adapt<Person, PersonDTO>();
        }

        [GlobalSetup(Target = nameof(FecTest))]
        public void SetupFec()
        {
            _person = TestAdaptHelper.SetupPersonInstance();
            TestAdaptHelper.UseMapsterCompiler(MapsterCompilerType.FEC);
            _ = _person.Adapt<Person, PersonDTO>();
        }

        [GlobalSetup(Target = nameof(CodegenTest))]
        public void SetupCodegen() => _person = TestAdaptHelper.SetupPersonInstance();

        [GlobalSetup(Target = nameof(AutoMapperTest))]
        public void SetupAutoMapper() => _person = TestAdaptHelper.SetupPersonInstance();

        [GlobalSetup(Target = nameof(FacetTest))]
        public void SetupFacet() => _person = TestAdaptHelper.SetupPersonInstance();

        [GlobalSetup(Target = nameof(FacetCompiledTest))]
        public void SetupFacetCompiled() => _person = TestAdaptHelper.SetupPersonInstance();

        [GlobalSetup(Target = nameof(MapperlyTest))]
        public void SetupMapperly() => _person = TestAdaptHelper.SetupPersonInstance();
    }
}