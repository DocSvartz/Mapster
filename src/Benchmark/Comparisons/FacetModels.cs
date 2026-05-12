using Facet;
using Mapster.Benchmark.Classes;

namespace Mapster.Benchmark.Comparisons
{
    [Facet(typeof(Foo), NestedFacets = new[] { typeof(FooFacetDto) }, MaxDepth = 2)]
    public partial class FooFacetDto
    {
    }

    [Facet(typeof(Address))]
    public partial class AddressFacetDto
    {
    }

    [Facet(typeof(Customer), NestedFacets = new[] { typeof(AddressFacetDto) })]
    public partial class CustomerFacetDto
    {
        [MapFrom("Address.City")]
        public string AddressCity { get; set; }
    }
}