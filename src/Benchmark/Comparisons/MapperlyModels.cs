using Mapster.Benchmark.Classes;
using Riok.Mapperly.Abstractions;

namespace Mapster.Benchmark.Comparisons
{
    public class FooMapperlyDto
    {
        public string Name { get; set; }
        public int Int32 { get; set; }
        public long Int64 { get; set; }
        public int? NullInt { get; set; }
        public float Floatn { get; set; }
        public double Doublen { get; set; }
        public DateTime DateTime { get; set; }
        public FooMapperlyDto Foo1 { get; set; }
        public IEnumerable<FooMapperlyDto> Foos { get; set; }
        public FooMapperlyDto[] FooArr { get; set; }
        public int[] IntArr { get; set; }
        public IEnumerable<int> Ints { get; set; }
    }

    public class AddressMapperlyDto
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class AddressSummaryMapperlyDto
    {
        public int Id { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
    }

    public class CustomerMapperlyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public AddressMapperlyDto Address { get; set; }
        public AddressSummaryMapperlyDto HomeAddress { get; set; }
        public AddressSummaryMapperlyDto[] Addresses { get; set; }
        public List<AddressSummaryMapperlyDto> WorkAddresses { get; set; }
        public string AddressCity { get; set; }
    }

    [Riok.Mapperly.Abstractions.Mapper(UseDeepCloning = true)]
    public static partial class MapperlyMappings
    {
        public static partial AddressMapperlyDto MapAddress(Address source);

        [MapperIgnoreSource(nameof(Address.Street))]
        public static partial AddressSummaryMapperlyDto MapAddressSummary(Address source);

        public static partial FooMapperlyDto MapFoo(Foo source);

        [MapperIgnoreSource(nameof(Customer.Credit))]
        [MapProperty("Address.City", nameof(CustomerMapperlyDto.AddressCity))]
        public static partial CustomerMapperlyDto MapCustomer(Customer source);
    }
}