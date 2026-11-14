using Mapster.Models;
using System;
using System.Collections.Generic;

namespace Mapster
{
    public class TypeAdapterConfigSnapshot : BaseConfigSnapshot
    {
        public override IReadOnlyCollection<TypeAdapterRule> DefaultSettingsRules { get; init; }
        public override IReadOnlyCollection<TypeAdapterRule> AdapterRules { get; init; }
        public override IReadOnlyCollection<TypeAdapterRule> CustomSettingsRules { get; init; } = Array.Empty<TypeAdapterRule>(); // need refactoring
        public override IReadOnlyDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; init; }
    }
}
