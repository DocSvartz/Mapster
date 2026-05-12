using BenchmarkDotNet.Attributes;
using Mapster.Benchmark.Classes;

namespace Mapster.Benchmark.Benchmarks
{
    public class TestSimpleTypes : MappingBenchmarkBase
    {
        private Foo _fooInstance;

        [Benchmark(Baseline = true, Description = $"Mapster {TestAdaptHelper.MapsterVersion}")]
        public void MapsterTest()
        {
            TestAdaptHelper.TestMapsterAdapter<Foo, Foo>(_fooInstance, MapOperations);
        }

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Roslyn)")]
        public void RoslynTest()
        {
            TestAdaptHelper.TestMapsterAdapter<Foo, Foo>(_fooInstance, MapOperations);
        }

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (FEC)")]
        public void FecTest()
        {
            TestAdaptHelper.TestMapsterAdapter<Foo, Foo>(_fooInstance, MapOperations);
        }

        [Benchmark(Description = $"Mapster {TestAdaptHelper.MapsterVersion} (Codegen)")]
        public void CodegenTest()
        {
            TestAdaptHelper.TestCodeGen(_fooInstance, MapOperations);
        }

        [Benchmark(Description = $"AutoMapper {TestAdaptHelper.AutoMapperVersion}")]
        public void AutoMapperTest()
        {
            TestAdaptHelper.TestAutoMapper<Foo, Foo>(_fooInstance, MapOperations);
        }

        [Benchmark(Description = $"Facet {TestAdaptHelper.FacetVersion}")]
        public void FacetTest()
        {
            TestAdaptHelper.TestFacet(_fooInstance, MapOperations);
        }

        [Benchmark(Description = $"Mapperly {TestAdaptHelper.MapperlyVersion}")]
        public void MapperlyTest()
        {
            TestAdaptHelper.TestMapperly(_fooInstance, MapOperations);
        }

        [GlobalSetup(Target = nameof(MapsterTest))]
        public void SetupMapster()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            TestAdaptHelper.ConfigureMapster<Foo, Foo>(_fooInstance, MapsterCompilerType.Default);
        }

        [GlobalSetup(Target = nameof(RoslynTest))]
        public void SetupRoslyn()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            TestAdaptHelper.ConfigureMapster<Foo, Foo>(_fooInstance, MapsterCompilerType.Roslyn);
        }

        [GlobalSetup(Target = nameof(FecTest))]
        public void SetupFec()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            TestAdaptHelper.ConfigureMapster<Foo, Foo>(_fooInstance, MapsterCompilerType.FEC);
        }

        [GlobalSetup(Target = nameof(CodegenTest))]
        public void SetupCodegen()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            _ = FooMapper.Map(_fooInstance);
        }

        [GlobalSetup(Target = nameof(FacetTest))]
        public void SetupFacet()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            TestAdaptHelper.ConfigureFacet(_fooInstance);
        }

        [GlobalSetup(Target = nameof(MapperlyTest))]
        public void SetupMapperly()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            TestAdaptHelper.ConfigureMapperly(_fooInstance);
        }

        [GlobalSetup(Target = nameof(AutoMapperTest))]
        public void SetupAutoMapper()
        {
            _fooInstance = TestAdaptHelper.SetupFooInstance();
            TestAdaptHelper.ConfigureAutoMapper<Foo, Foo>(_fooInstance);
        }
    }
}