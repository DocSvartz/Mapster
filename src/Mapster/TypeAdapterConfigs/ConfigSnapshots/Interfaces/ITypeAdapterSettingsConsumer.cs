using Mapster.Models;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Mapster
{
    public interface ITypeAdapterSettingsConsumer : IGetReadOnlyRules
    {
        ImmutableArray<TypeAdapterRule> TemplateRules { get; init; }
        IReadOnlyCollection<TypeAdapterRule> AdapterRules { get; init; }
        IReadOnlyCollection<TypeAdapterRule> CustomSettingsRules { get; init; }
        IReadOnlyDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; init; }
    }

    public interface IGetReadOnlyRules : IReadOnlyCollection<TypeAdapterRule> { }
}
