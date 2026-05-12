using AutoMapper;
using FastExpressionCompiler;
using Mapster.Benchmark.Classes;
using Mapster.Benchmark.Comparisons;
using System.Linq.Expressions;

namespace Mapster.Benchmark
{
    public static class TestAdaptHelper
    {
        private static readonly MapperConfiguration AutoMapperConfiguration = new(cfg =>
        {
            cfg.CreateMap<Foo, Foo>();
            cfg.CreateMap<Address, Address>();
            cfg.CreateMap<Address, AddressDTO>();
            cfg.CreateMap<Customer, CustomerDTO>()
                .ForMember(destination => destination.AddressCity,
                    options => options.MapFrom(source => source.Address != null ? source.Address.City : null));
        });

        private static readonly IMapper AutoMapperInstance = CreateAutoMapper();
        private static readonly Func<LambdaExpression, Delegate> DefaultCompiler = TypeAdapterConfig.GlobalSettings.Compiler;

        public const string MapsterVersion = "10.0.7";
        public const string AutoMapperVersion = "14.0.0";
        public const string FacetVersion = "6.5.5";
        public const string MapperlyVersion = "4.3.1";

        public static Customer SetupCustomerInstance()
        {
            return new Customer
            {
                Address = new Address { City = "istanbul", Country = "turkey", Id = 1, Street = "istiklal cad." },
                HomeAddress = new Address { City = "istanbul", Country = "turkey", Id = 2, Street = "istiklal cad." },
                Id = 1,
                Name = "Eduardo Najera",
                Credit = 234.7m,
                WorkAddresses = new List<Address>
                {
                    new Address {City = "istanbul", Country = "turkey", Id = 5, Street = "istiklal cad."},
                    new Address {City = "izmir", Country = "turkey", Id = 6, Street = "konak"}
                },
                Addresses = new[]
                {
                    new Address {City = "istanbul", Country = "turkey", Id = 3, Street = "istiklal cad."},
                    new Address {City = "izmir", Country = "turkey", Id = 4, Street = "konak"}
                }
            };
        }

        public static Foo SetupFooInstance()
        {
            return new Foo
            {
                Name = "foo",
                Int32 = 12,
                Int64 = 123123,
                NullInt = 16,
                DateTime = DateTime.Now,
                Doublen = 2312112,
                Foo1 = new Foo { Name = "foo one" },
                Foos = new List<Foo>
                {
                    new Foo {Name = "j1", Int64 = 123, NullInt = 321},
                    new Foo {Name = "j2", Int32 = 12345, NullInt = 54321},
                    new Foo {Name = "j3", Int32 = 12345, NullInt = 54321}
                },
                FooArr = new[]
                {
                    new Foo {Name = "a1"},
                    new Foo {Name = "a2"},
                    new Foo {Name = "a3"}
                },
                IntArr = new[] { 1, 2, 3, 4, 5 },
                Ints = new[] { 7, 8, 9 }
            };
        }

        private static IMapper CreateAutoMapper()
        {
            AutoMapperConfiguration.AssertConfigurationIsValid();
            AutoMapperConfiguration.CompileMappings();
            return AutoMapperConfiguration.CreateMapper();
        }

        private static void SetupCompiler(MapsterCompilerType type)
        {
            TypeAdapterConfig.GlobalSettings.Compiler = type switch
            {
                MapsterCompilerType.Default => DefaultCompiler,
                MapsterCompilerType.Roslyn => expression => expression.CompileWithDebugInfo(),
                MapsterCompilerType.FEC => expression => expression.CompileFast(),
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }

        public static void ConfigureMapster<TSource, TDestination>(TSource sourceInstance, MapsterCompilerType type)
            where TSource : class
            where TDestination : class
        {
            SetupCompiler(type);
            TypeAdapterConfig.GlobalSettings.Compile(typeof(TSource), typeof(TDestination));
            _ = sourceInstance.Adapt<TSource, TDestination>();
        }

        public static void ConfigureAutoMapper<TSource, TDestination>(TSource sourceInstance)
            where TSource : class
        {
            _ = AutoMapperInstance.Map<TSource, TDestination>(sourceInstance);
        }

        public static void ConfigureFacet(Foo sourceInstance)
        {
            _ = new FooFacetDto(sourceInstance);
        }

        public static void ConfigureFacet(Customer sourceInstance)
        {
            _ = new CustomerFacetDto(sourceInstance);
        }

        public static void ConfigureMapperly(Foo sourceInstance)
        {
            _ = MapperlyMappings.MapFoo(sourceInstance);
        }

        public static void ConfigureMapperly(Customer sourceInstance)
        {
            _ = MapperlyMappings.MapCustomer(sourceInstance);
        }

        public static void TestMapsterAdapter<TSrc, TDest>(TSrc item, int mapOperations)
            where TSrc : class
            where TDest : class, new()
        {
            Loop(item, source => source.Adapt<TSrc, TDest>(), mapOperations);
        }

        public static void TestAutoMapper<TSrc, TDest>(TSrc item, int mapOperations)
            where TSrc : class
            where TDest : class, new()
        {
            Loop(item, source => AutoMapperInstance.Map<TSrc, TDest>(source), mapOperations);
        }

        public static void TestFacet(Foo item, int mapOperations)
        {
            Loop(item, source => new FooFacetDto(source), mapOperations);
        }

        public static void TestFacet(Customer item, int mapOperations)
        {
            Loop(item, source => new CustomerFacetDto(source), mapOperations);
        }

        public static void TestMapperly(Foo item, int mapOperations)
        {
            Loop(item, MapperlyMappings.MapFoo, mapOperations);
        }

        public static void TestMapperly(Customer item, int mapOperations)
        {
            Loop(item, MapperlyMappings.MapCustomer, mapOperations);
        }

        public static void TestCodeGen(Foo item, int mapOperations)
        {
            Loop(item, FooMapper.Map, mapOperations);
        }

        public static void TestCodeGen(Customer item, int mapOperations)
        {
            Loop(item, CustomerMapper.Map, mapOperations);
        }

        private static void Loop<TSource, TDestination>(TSource item, Func<TSource, TDestination> map, int mapOperations)
        {
            TDestination result = default!;
            for (var i = 0; i < mapOperations; i++)
            {
                result = map(item);
            }

            GC.KeepAlive(result);
        }
    }

    public enum MapsterCompilerType
    {
        Default,
        Roslyn,
        FEC,
    }
}