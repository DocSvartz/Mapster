using Mapster.Models;
using System.Collections.Generic;

namespace Mapster
{
    /// <summary>
    /// This class emulates the work of the original Mapping settings store used before version 10.1.
    /// </summary>
    internal class RunningConfigSnapshot : BaseConfigSnapshot
    {
        public override IReadOnlyCollection<TypeAdapterRule> DefaultSettingsRules { get; init; }
        public override IReadOnlyCollection<TypeAdapterRule> AdapterRules { get; init; }
        public override IReadOnlyCollection<TypeAdapterRule> CustomSettingsRules { get; init; } // need refactoring
        public override IReadOnlyDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; init; }
    }
}
