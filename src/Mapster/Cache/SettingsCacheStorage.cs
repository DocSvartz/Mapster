using Mapster.Adapters;
using Mapster.Models;
using Mapster.TypeAdapterSetters;
using Mapster.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Mapster.Cache
{
    public partial class SettingsCacheStorage
    {
        #region PrivateMembers 
        private readonly TypeAdapterConfig _config;
        private readonly object _lock = new();
        private ulong _Version = 0;
        private TypeAdapterRule DefaultSettingsRule;
        private ImmutableArray<TypeAdapterRule> AdapterRules;
        private ImmutableDictionary<TypeTuple, TypeAdapterRule> RuleMap = ImmutableDictionary<TypeTuple, TypeAdapterRule>.Empty;
        private ImmutableDictionary<string, bool> BoolSettings = ImmutableDictionary<string, bool>.Empty;
        private readonly Dictionary<CompiledTypesSettings, TypeAdapterRule> CompiledRule = new();

        private readonly Dictionary<Type, IMemberModelEx[]> TypeMemberModelsCache = new();

        private TypeAdapterConfigSnapshot? _cachedSnapShot;
        
        private readonly Dictionary<TypeTuple, Delegate> _mapCache = new();
        private readonly Dictionary<TypeTuple, Delegate> _mapToTargetCache = new();
        private readonly Dictionary<TypeTuple, Delegate> _ProjectionCache = new();
        private readonly Dictionary<TypeTuple, Delegate> _dynamicMapCache = new();

        private ulong UpdateAndGetVersion() { _Version = ++_Version; return _Version; }
        private void UpdateVersion() { _Version = ++_Version; }

        private static TypeAdapterRule CreateDefaultRule(TypeAdapterSettings? settings = null)
        {
            Func<PreCompileArgument, int?> priority = arg => -100;

            return settings == null
                ? new ExplicitTypeAdapterRule(priority)
                : new ExplicitTypeAdapterRule(priority, settings);
        }


        private static TypeAdapterRule CreateDestinationTypeRule(TypeTuple key, TypeAdapterSettings? settings = null)
        {
            Func<PreCompileArgument, int?> priority = arg => arg.DestinationType.GetSubclassDistance(key.Destination, true);

            return settings == null
                ? new ExplicitTypeAdapterRule(priority)
                : new ExplicitTypeAdapterRule(priority, settings);
        }
        private TypeAdapterRule CreateTypeTupleRule(TypeTuple key, TypeAdapterSettings? settings = null)
        {
            Func<PreCompileArgument, int?> priority =
                arg =>
                {
                    var score1 = arg.DestinationType.GetSubclassDistance(key.Destination, GetBoolSetting(nameof(TypeAdapterConfig.AllowImplicitDestinationInheritance)));
                    if (score1 == null)
                        return null;
                    var score2 = arg.SourceType.GetSubclassDistance(key.Source, GetBoolSetting(nameof(TypeAdapterConfig.AllowImplicitSourceInheritance)));
                    if (score2 == null)
                        return null;
                    return score1.Value + score2.Value;
                };

            return settings == null
                ? new ExplicitTypeAdapterRule(priority)
                : new ExplicitTypeAdapterRule(priority, settings);
        }

        private TypeAdapterSettings GetSettings(TypeTuple types, bool isForceNewConfig = false)
        {
            TypeAdapterSettings? clonedConfig = null;
            TypeAdapterRule? ruleMap = null;

            if (!isForceNewConfig)
            {
                RuleMap.TryGetValue(types, out ruleMap);
                clonedConfig = ruleMap?.Settings.Clone();
            }

            if (types.Source == typeof(MapsterCacheMarker) 
                && types.Destination == typeof(DefaultOnlySetter))
                    ruleMap = CreateDefaultRule(clonedConfig);
            else
            {
                ruleMap = types.Source == typeof(void)
                   ? CreateDestinationTypeRule(types, clonedConfig)
                   : CreateTypeTupleRule(types, clonedConfig);
            }

            ruleMap.Settings.Id = UpdateAndGetVersion();
            //Rules.Add(ruleMap);

            RuleMap = RuleMap.SetItem(types, ruleMap);
                      
            ruleMap.Settings.SourceType = types.Source;
            ruleMap.Settings.DestinationType = types.Destination;

            return ruleMap.Settings;
        }

        //private void Remove(TypeTuple key)
        //{
        //    RuleMap.TryGetValue(key, out var rule);
        //    if (rule != null)
        //    {
        //       // Rules.Remove(rule);
        //       // RuleMap.Remove(key);
        //    }
        //  //  _mapCache.Remove(key);
        //  //  _mapToTargetCache.Remove(key);
        //  //  _ProjectionCache.Remove(key);
        //  //  _dynamicMapCache.Remove(key);
        //}

        #endregion PrivateMembers


        public IReadOnlyCollection<TypeAdapterRule> Rules => this;
        public SettingsCacheStorage(ImmutableArray<TypeAdapterRule> ruleTemplates, TypeAdapterConfig config)
        {
            AdapterRules = ruleTemplates;
            var setting = TypeAdapterSettings.CreateDefault;
            _config = config;
        }

        internal SettingsCacheStorage(TypeAdapterConfigSnapshot configSnapshot, TypeAdapterConfig config)
        {
            _Version = configSnapshot.Version;
            AdapterRules = configSnapshot.AdapterRules;
            RuleMap = configSnapshot.RuleMap;
            BoolSettings = configSnapshot.BoolSettings;
            CompiledRule = configSnapshot.CompiledRule;
            TypeMemberModelsCache = configSnapshot.TypeMemberModelsCache;
            _config = config;
        }

        public void SetBoolSetting(string name, bool value)
        {
            lock (_lock)
            {
                BoolSettings.TryGetValue(name, out var data);

                if(value != data)
                {
                    BoolSettings = BoolSettings.SetItem(name, value);
                    UpdateVersion();
                }
            }
        }
 

        public bool GetBoolSetting(string name) { BoolSettings.TryGetValue(name, out var data); return data; }
                
        public TypeAdapterSetter<TSource, TDestination> GetNewConfigSetter<TSource, TDestination>(TypeTuple tuple, TypeAdapterConfig config)
        {
            lock (_lock)
            {
                Remove(tuple);
                return new TypeAdapterSetter<TSource, TDestination>(GetSettings(tuple,true), config);
            }
        }

        public TypeAdapterSetter GetNewConfigSetter(TypeTuple tuple, TypeAdapterConfig config)
        {
            lock (_lock)
            {
                Remove(tuple);
                return new TypeAdapterSetter(GetSettings(tuple,true), config);
            }
        }

        public TypeAdapterSetter<TSource, TDestination> GetForTypeSetter<TSource, TDestination>(TypeTuple tuple, TypeAdapterConfig config)
        {
            lock (_lock)
            {
                return new TypeAdapterSetter<TSource, TDestination>(GetSettings(tuple), config);
            }
        }

        public TypeAdapterSetter GetForTypeSetter(TypeTuple tuple, TypeAdapterConfig config)
        {
            lock (_lock)
            {
                return new TypeAdapterSetter(GetSettings(tuple), config);
            }
        }

        public TypeAdapterSetter<TDestination> GetForDestinationTypeSetter<TDestination>(TypeTuple tuple, TypeAdapterConfig config)
        {
            lock (_lock)
            {
                return new TypeAdapterSetter<TDestination>(GetSettings(tuple), config);
            }
        }

        public DestinationTypeOnlySetter GetForDestinationTypeSetter(TypeTuple tuple, TypeAdapterConfig config)
        {
            lock (_lock)
            {
                return new DestinationTypeOnlySetter(GetSettings(tuple), config);
            }
        }



        public void AddAdapterRule(BaseAdapter adapter)
        {
            lock (_lock)
            {
                AdapterRules = AdapterRules.Add(adapter.CreateRule());
                UpdateVersion();
            }
        }

        public void AddRule(TypeAdapterRule rule)
        {
            lock (_lock)
            {
                AdapterRules = AdapterRules.Add(rule);
                UpdateVersion();
            }
        }

        public ImmutableDictionary<TypeTuple, TypeAdapterRule> GetRuleMap
        {
            get
            {
                lock (_lock)
                {
                    // need cache 
                    return ImmutableDictionary.CreateRange<TypeTuple, TypeAdapterRule>(RuleMap);
                }
            }
        }

        public Dictionary<Type, IMemberModelEx[]> GetTypeMemberModelsCache => TypeMemberModelsCache;

        public TypeAdapterRule GetCompiledRule(TypeTuple types, TypeAdapterSettings settings)
        {
            var ruleMap = types.Source == typeof(void)
                 ? CreateDestinationTypeRule(types, settings)
                 : CreateTypeTupleRule(types, settings);

            return ruleMap;
        }

        public void CollectCompileRules(IEnumerable<TypeAdapterRule> rules)
        {
            lock (_lock)
            {
                foreach (var item in rules)
                {
                    if(item.Settings is CompiledTypesSettings settings)
                        CompiledRule.TryAdd(settings, item);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                RuleMap = RuleMap.Clear();
                UpdateVersion();
            }
        }

        public void Remove(TypeTuple types)
        {
            lock (_lock)
            {
                RuleMap = RuleMap.Remove(types);
                UpdateVersion();
            }
        }

        private TypeAdapterConfigSnapshot GetSnapShot() 
        {
            lock ( _lock)
            {
                if (_cachedSnapShot == null || _cachedSnapShot?.Version != _Version)
                    _cachedSnapShot = new TypeAdapterConfigSnapshot()
                    {
                        Version = this._Version,
                        BoolSettings = this.BoolSettings,
                        AdapterRules = this.AdapterRules,
                        RuleMap = this.RuleMap,
                        CompiledRule = this.CompiledRule, // need refactoring
                        Compiler = _config.Compiler,
                        Config = _config,
                        TypeMemberModelsCache = this.TypeMemberModelsCache,
                    };

                return _cachedSnapShot;
            }
        }

         

        public TypeAdapterConfigSnapshot CurrentSnapshot => GetSnapShot();
    }
}
