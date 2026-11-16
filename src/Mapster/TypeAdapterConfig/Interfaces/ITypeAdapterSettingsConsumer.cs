using Mapster.Models;
using System.Collections.Generic;

namespace Mapster
{
    public interface ITypeAdapterSettingsConsumer : IGetReadOnlyRules
    {
        IReadOnlyCollection<TypeAdapterRule> DefaultSettingsRules { get; init; }
        IReadOnlyCollection<TypeAdapterRule> AdapterRules { get; init; }
        IReadOnlyCollection<TypeAdapterRule> CustomSettingsRules { get; init; }
        IReadOnlyDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; init; }
    }

    public interface IGetReadOnlyRules : IReadOnlyCollection<TypeAdapterRule> { }
}
