using Mapster.Cache;
using Mapster.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Mapster
{
    public interface ITypeAdapterConfigSnapshot : ITypeAdapterMappingFuncProducer, ITypeAdapterSettingsConsumer
    {
        ulong Version { get; init; }
        internal ITypeAdapterConfig Config { get; init; }
        IReadOnlyCollection<TypeAdapterRule> Rules { get; }
        public TypeAdapterRule DefaultSettingsRule { get; init; }

        internal ConcurrentDictionary<Type, MappingTypeModel> TypeMemberModelsCache { get; init; }
        internal ConcurrentDictionary<TypeTuple, CompiledSettingsCacheItem> CacheCompiledTypesSettings { get; init; }
        internal ImmutableDictionary<string, bool> BoolSettings { get; init; }

        bool GetBoolSetting(string name);

       
        void Compile(Type sourceType, Type destinationType);
        void Compile(bool failFast);
        void CompileProjection(Type sourceType, Type destinationType);
    }
}
