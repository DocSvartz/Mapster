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
    internal class RunningConfigSnapshot : BaseConfigSnapshot, ITypeAdapterConfigSnapshot
    {
        public override IReadOnlyCollection<TypeAdapterRule> DefaultSettingsRules { get; init; }
        public override IReadOnlyCollection<TypeAdapterRule> AdapterRules { get; init; }
        public override IReadOnlyCollection<TypeAdapterRule> CustomSettingsRules { get; init; }
        public override IReadOnlyDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; init; }


        ConcurrentDictionary<Type, MappingTypeModel> ITypeAdapterConfigSnapshot.TypeMemberModelsCache { get => TypeMemberModelsCache; init => TypeMemberModelsCache = value; }
        ConcurrentDictionary<TypeTuple, CompiledSettingsCacheItem> ITypeAdapterConfigSnapshot.CacheCompiledTypesSettings { get => CacheCompiledTypesSettings; init => CacheCompiledTypesSettings = value; }
        ImmutableDictionary<string, bool> ITypeAdapterConfigSnapshot.BoolSettings { get => BoolSettings; init => BoolSettings = value; }
        IReadOnlyCollection<TypeAdapterRule> ITypeAdapterSettingsConsumer.DefaultSettingsRules { get => DefaultSettingsRules; init => throw new NotImplementedException(); }
        ITypeAdapterConfig ITypeAdapterConfigSnapshot.Config { get => Config; init => Config = value; }
    }
}
