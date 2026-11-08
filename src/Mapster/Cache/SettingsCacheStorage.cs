using Mapster.Adapters;
using Mapster.Models;
using Mapster.TypeAdapterSetters;
using Mapster.Utils;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Mapster.Cache
{
    public class SettingsCacheStorage
    {
        private readonly object _lock = new();
        private ulong _Id = 0;
        private readonly ImmutableArray<TypeAdapterRule> ImmutableRules;
        private readonly List<TypeAdapterRule> Rules = new();
        private readonly Dictionary<TypeTuple, TypeAdapterRule> RuleMap = new();
        private readonly Dictionary<Type, IMemberModelEx[]> TypeMemberModelsCache = new();


        private readonly Dictionary<CompiledTypesSettings, TypeAdapterRule> CompiledRule = new();
        private readonly Dictionary<TypeTuple, Delegate> _mapCache = new();
        private readonly Dictionary<TypeTuple, Delegate> _mapToTargetCache = new();
        private readonly Dictionary<TypeTuple, Delegate> _ProjectionCache = new();
        private readonly Dictionary<TypeTuple, Delegate> _dynamicMapCache = new();
        private bool allowImplicitDestinationInheritance;
        private bool allowImplicitSourceInheritance;
               
        private static TypeAdapterRule CreateDefaultRule(TypeAdapterSettings? settings = null)
        {
          return  new TypeAdapterRule
            {
                Priority = arg => -100,
                Settings = settings ?? new TypeAdapterSettings()
            };
        }


        private static TypeAdapterRule CreateDestinationTypeRule(TypeTuple key, TypeAdapterSettings? settings = null)
        {
            return new TypeAdapterRule
            {
                Priority = arg => arg.DestinationType.GetSubclassDistance(key.Destination, true),
                Settings = settings ?? new TypeAdapterSettings(),
            };
        }
        private TypeAdapterRule CreateTypeTupleRule(TypeTuple key, TypeAdapterSettings? settings = null)
        {
            return new TypeAdapterRule
            {
                Priority = arg =>
                {
                    var score1 = arg.DestinationType.GetSubclassDistance(key.Destination, AllowImplicitDestinationInheritance);
                    if (score1 == null)
                        return null;
                    var score2 = arg.SourceType.GetSubclassDistance(key.Source, AllowImplicitSourceInheritance);
                    if (score2 == null)
                        return null;
                    return score1.Value + score2.Value;
                },
                Settings = settings ?? new TypeAdapterSettings(),
            };
        }

        private TypeAdapterSettings GetSettings(TypeTuple types)
        {
            RuleMap.TryGetValue(types, out var ruleMap);

            var clonedConfig = ruleMap?.Settings.Clone();

            if (types.Source == typeof(MapsterCacheMarker) 
                && types.Destination == typeof(DefaultOnlySetter))
                    ruleMap = CreateDefaultRule(clonedConfig);
            else
            {
                ruleMap = types.Source == typeof(void)
                   ? CreateDestinationTypeRule(types, clonedConfig)
                   : CreateTypeTupleRule(types, clonedConfig);
            }

            ruleMap.Settings.Id = ++_Id;
            //Rules.Add(ruleMap);
            RuleMap[types] = ruleMap;
            
            ruleMap.Settings.SourceType = types.Source;
            ruleMap.Settings.DestinationType = types.Destination;

            return ruleMap.Settings;
        }

        private void Remove(TypeTuple key)
        {
            RuleMap.TryGetValue(key, out var rule);
            if (rule != null)
            {
               // Rules.Remove(rule);
                RuleMap.Remove(key);
            }
            _mapCache.Remove(key);
            _mapToTargetCache.Remove(key);
            _ProjectionCache.Remove(key);
            _dynamicMapCache.Remove(key);
        }

        public bool AllowImplicitDestinationInheritance
        {
            get => allowImplicitDestinationInheritance;
            set
            {
                lock (_lock)
                {
                    allowImplicitDestinationInheritance = value;
                }
            }
        }
        public bool AllowImplicitSourceInheritance 
        { 
            get => allowImplicitSourceInheritance; 
            set
            {
                lock(_lock)
                {
                    allowImplicitSourceInheritance = value;
                }
            }
        
        }

        public SettingsCacheStorage(ImmutableArray<TypeAdapterRule> ruleTemplates, TypeAdapterConfig config)
        {
            ImmutableRules = ruleTemplates;
            var setting = TypeAdapterSettings.CreateDefault;
        }

        public TypeAdapterSetter<TSource, TDestination> GetNewConfigSetter<TSource, TDestination>(TypeTuple tuple, TypeAdapterConfig config)
        {
            lock (_lock)
            {
                Remove(tuple);
                return new TypeAdapterSetter<TSource, TDestination>(GetSettings(tuple), config);
            }
        }

        public TypeAdapterSetter GetNewConfigSetter(TypeTuple tuple, TypeAdapterConfig config)
        {
            lock (_lock)
            {
                Remove(tuple);
                return new TypeAdapterSetter(GetSettings(tuple), config);
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
                Rules.Add(adapter.CreateRule());
            }
        }

        public void AddRule(TypeAdapterRule rule)
        {
            lock (_lock)
            {
                Rules.Add(rule);
            }
        }


        public ImmutableList<TypeAdapterRule> GetRules
        {
            get
            {
                lock (_lock)
                {
                    // need cache 
                    return ImmutableList.CreateRange<TypeAdapterRule>(ImmutableRules.Concat(Rules).Concat(RuleMap.Select(x=> x.Value)));
                }
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

        internal (ImmutableList<TypeAdapterRule> Rules, 
                ImmutableDictionary<TypeTuple, TypeAdapterRule> RuleMap,
                ImmutableDictionary<CompiledTypesSettings, TypeAdapterRule> CompiledRule) GetFromMergedSettingsBundle()
        {
            lock ( _lock)
            {
              return  (ImmutableList.CreateRange<TypeAdapterRule>(ImmutableRules.Concat(Rules).Concat(RuleMap.Select(x => x.Value))),
                    ImmutableDictionary.CreateRange<TypeTuple, TypeAdapterRule>(RuleMap),
                    ImmutableDictionary.CreateRange<CompiledTypesSettings, TypeAdapterRule>(CompiledRule));
            }
        }
    }
}
