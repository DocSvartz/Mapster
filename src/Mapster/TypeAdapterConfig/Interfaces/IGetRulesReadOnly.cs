using Mapster.Models;
using System.Collections.Generic;

namespace Mapster
{
    public interface IGetRulesReadOnly : IReadOnlyCollection<TypeAdapterRule>
    {
        TypeAdapterRule DefaultSettingsRule { get; }
        IReadOnlyCollection<TypeAdapterRule> DefaultSettingsRules { get; }
        IReadOnlyCollection<TypeAdapterRule> AdapterRules { get; }
        IReadOnlyCollection<TypeAdapterRule> CustomSettingsRules { get; }
        IReadOnlyDictionary<TypeTuple,TypeAdapterRule> RuleMap { get; }

    }
}
