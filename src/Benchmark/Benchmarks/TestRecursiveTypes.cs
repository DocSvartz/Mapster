using BenchmarkDotNet.Attributes;
using Mapster.Benchmark.Classes;
using Mapster.Benchmark.Comparisons;

namespace Mapster.Benchmark.Benchmarks
{
    // Self-recursive graph with nested references and collections.
    // Source: Foo, Destination: FooDTO.
    public class TestRecursiveTypes : MappingBenchmarkBase
    {
        private static readonly Func<Foo, FooFacetDto> FooFacetCompiled = FooFacetDto.Projection.Compile();

        private Foo _foo;

        [Benchmark(Baseline = true, Description = $"Mapster {TestAdaptHelper.MapsterVersion}")]
        public void MapsterTest()
            => TestAdaptHelper.Loop(_foo, src => src.Adapt<Foo, FooDTO>(), MapOperations);

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Roslyn)")]
        public void RoslynTest()
            => TestAdaptHelper.Loop(_foo, src => src.Adapt<Foo, FooDTO>(), MapOperations);

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (FEC)")]
        public void FecTest()
            => TestAdaptHelper.Loop(_foo, src => src.Adapt<Foo, FooDTO>(), MapOperations);

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Codegen)")]
        public void CodegenTest()
            => TestAdaptHelper.Loop(_foo, FooMapper.Map, MapOperations);

        [Benchmark(Description = $"AutoMapper {TestAdaptHelper.AutoMapperVersion}")]
        public void AutoMapperTest()
            => TestAdaptHelper.Loop(_foo, src => TestAdaptHelper.AutoMapper.Map<Foo, FooDTO>(src), MapOperations);

        [Benchmark(Description = $"Facet {TestAdaptHelper.FacetVersion}")]
        public void FacetTest()
            => TestAdaptHelper.Loop(_foo, src => new FooFacetDto(src), MapOperations);

        [Benchmark(Description = $"Facet {TestAdaptHelper.FacetVersion} (Compiled Projection)")]
        public void FacetCompiledTest()
            => TestAdaptHelper.Loop(_foo, FooFacetCompiled, MapOperations);

        [Benchmark(Description = $"Mapperly {TestAdaptHelper.MapperlyVersion}")]
        public void MapperlyTest()
            => TestAdaptHelper.Loop(_foo, MapperlyMappings.MapFoo, MapOperations);

        [GlobalSetup(Target = nameof(MapsterTest))]
        public void SetupMapster()
        {
            _foo = TestAdaptHelper.SetupFooInstance();
            TestAdaptHelper.UseMapsterCompiler(MapsterCompilerType.Default);
            _ = _foo.Adapt<Foo, FooDTO>();
        }

        [GlobalSetup(Target = nameof(RoslynTest))]
        public void SetupRoslyn()
        {
            _foo = TestAdaptHelper.SetupFooInstance();
            TestAdaptHelper.UseMapsterCompiler(MapsterCompilerType.Roslyn);
            _ = _foo.Adapt<Foo, FooDTO>();
        }

        [GlobalSetup(Target = nameof(FecTest))]
        public void SetupFec()
        {
            _foo = TestAdaptHelper.SetupFooInstance();
            TestAdaptHelper.UseMapsterCompiler(MapsterCompilerType.FEC);
            _ = _foo.Adapt<Foo, FooDTO>();
        }

        [GlobalSetup(Target = nameof(CodegenTest))]
        public void SetupCodegen() => _foo = TestAdaptHelper.SetupFooInstance();

        [GlobalSetup(Target = nameof(AutoMapperTest))]
        public void SetupAutoMapper() => _foo = TestAdaptHelper.SetupFooInstance();

        [GlobalSetup(Target = nameof(FacetTest))]
        public void SetupFacet() => _foo = TestAdaptHelper.SetupFooInstance();

        [GlobalSetup(Target = nameof(FacetCompiledTest))]
        public void SetupFacetCompiled() => _foo = TestAdaptHelper.SetupFooInstance();

        [GlobalSetup(Target = nameof(MapperlyTest))]
        public void SetupMapperly() => _foo = TestAdaptHelper.SetupFooInstance();
    }
}