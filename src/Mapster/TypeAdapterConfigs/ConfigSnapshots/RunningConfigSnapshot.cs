using Mapster.Cache;
using Mapster.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Mapster
{
    /// <summary>
    /// This class emulates the work of the original Mapping settings store used before version 10.1.
    /// </summary>
    public sealed class RunningConfigSnapshot : BaseConfigSnapshot, ITypeAdapterConfigSnapshot
    {
        public override IReadOnlyCollection<TypeAdapterRule> DefaultSettingsRules { get; init; }
        public override IReadOnlyCollection<TypeAdapterRule> AdapterRules { get; init; }
        public override IReadOnlyCollection<TypeAdapterRule> CustomSettingsRules { get; init; }
        public override IReadOnlyDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; init; }
    }
}
