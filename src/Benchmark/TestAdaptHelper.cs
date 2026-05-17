using AutoMapper;
using FastExpressionCompiler;
using Mapster.Benchmark.Classes;
using System.Linq.Expressions;

namespace Mapster.Benchmark
{
    public enum MapsterCompilerType
    {
        Default,
        Roslyn,
        FEC,
    }

    /// <summary>
    /// Minimal shared helper for the comparison benchmarks. Contains only setup data,
    /// the AutoMapper instance, a Mapster compiler switch and a generic hot-loop driver. 
    /// </summary>
    public static class TestAdaptHelper
    {
        public const string MapsterVersion = "10.0.7";
        public const string AutoMapperVersion = "14.0.0";
        public const string FacetVersion = "6.5.5";
        public const string MapperlyVersion = "4.3.1";

        private static readonly Func<LambdaExpression, Delegate> DefaultMapsterCompiler =
            TypeAdapterConfig.GlobalSettings.Compiler;

        public static readonly IMapper AutoMapper = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Foo, FooDTO>();
            cfg.CreateMap<Address, Address>();
            cfg.CreateMap<Address, AddressDTO>();
            cfg.CreateMap<Customer, CustomerDTO>()
                .ForMember(d => d.AddressCity,
                    o => o.MapFrom(s => s.Address != null ? s.Address.City : null));
            cfg.CreateMap<Person, PersonDTO>();
        }).CreateMapper();

        public static Foo SetupFooInstance() => new Foo
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
                new Foo { Name = "j1", Int64 = 123, NullInt = 321 },
                new Foo { Name = "j2", Int32 = 12345, NullInt = 54321 },
                new Foo { Name = "j3", Int32 = 12345, NullInt = 54321 },
            },
            FooArr = new[]
            {
                new Foo { Name = "a1" },
                new Foo { Name = "a2" },
                new Foo { Name = "a3" },
            },
            IntArr = new[] { 1, 2, 3, 4, 5 },
            Ints = new[] { 7, 8, 9 },
        };

        public static Customer SetupCustomerInstance() => new Customer
        {
            Id = 1,
            Name = "Eduardo Najera",
            Credit = 234.7m,
            Address = new Address { Id = 1, City = "istanbul", Country = "turkey", Street = "istiklal cad." },
            HomeAddress = new Address { Id = 2, City = "istanbul", Country = "turkey", Street = "istiklal cad." },
            Addresses = new[]
            {
                new Address { Id = 3, City = "istanbul", Country = "turkey", Street = "istiklal cad." },
                new Address { Id = 4, City = "izmir",    Country = "turkey", Street = "konak" },
            },
            WorkAddresses = new List<Address>
            {
                new Address { Id = 5, City = "istanbul", Country = "turkey", Street = "istiklal cad." },
                new Address { Id = 6, City = "izmir",    Country = "turkey", Street = "konak" },
            },
        };

        public static Person SetupPersonInstance() => new Person
        {
            Id = 42,
            FirstName = "Eduardo",
            LastName = "Najera",
            Email = "eduardo@example.com",
            Age = 39,
            BirthDate = new DateTime(1986, 7, 11),
            Salary = 12345.67m,
            IsActive = true,
        };

        /// <summary>
        /// Switches Mapster's global expression compiler. Call this from a [GlobalSetup] before warming up the mapping.
        /// </summary>
        public static void UseMapsterCompiler(MapsterCompilerType type)
        {
            TypeAdapterConfig.GlobalSettings.Compiler = type switch
            {
                MapsterCompilerType.Default => DefaultMapsterCompiler,
                MapsterCompilerType.Roslyn => e => e.CompileWithDebugInfo(),
                MapsterCompilerType.FEC => e => e.CompileFast(),
                _ => throw new ArgumentOutOfRangeException(nameof(type)),
            };
        }

        /// <summary>
        /// Hot loop: invokes <paramref name="map"/> on <paramref name="src"/> <paramref name="count"/> times.
        /// Keeps the last result alive so the JIT can't dead-code-eliminate the call.
        /// </summary>
        public static void Loop<TSrc, TDest>(TSrc src, Func<TSrc, TDest> map, int count)
        {
            TDest r = default!;
            for (var i = 0; i < count; i++)
                r = map(src);

            GC.KeepAlive(r);
        }
    }
}
