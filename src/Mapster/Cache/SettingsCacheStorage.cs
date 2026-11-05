using Mapster.Adapters;
using Mapster.Models;
using Mapster.TypeAdapterSetters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mapster.Cache
{
    public class SettingsCacheStorage
    {
        private readonly object _lock = new();
        private ulong _Id = 0;
        private readonly List<TypeAdapterRule> ImmutableRules;
        private IDefaultTypeAdapterSettings DefaultSettings;
        private DefaultOnlySetter DefaultSetter;
        private readonly List<TypeAdapterRule> Rules = new();

        private readonly Dictionary<TypeTuple, TypeAdapterRule> RuleMap = new();
        private readonly Dictionary<Type, IMemberModelEx[]> TypeMemberModelsCache = new();

        private readonly Dictionary<TypeTuple, Delegate> _mapCache = new();
        private readonly Dictionary<TypeTuple, Delegate> _mapToTargetCache = new();
        private readonly Dictionary<TypeTuple, Delegate> _ProjectionCache = new();
        private readonly Dictionary<TypeTuple, Delegate> _dynamicMapCache = new();

        private bool _AllowImplicitDestinationInheritance;
        private bool _AllowImplicitSourceInheritance;

        private static TypeAdapterRule CreateDestinationTypeRule(TypeTuple key)
        {
            return new TypeAdapterRule
            {
                Priority = arg => GetSubclassDistance(arg.DestinationType, key.Destination, true),
                Settings = new TypeAdapterSettings(),
            };
        }
        private TypeAdapterRule CreateTypeTupleRule(TypeTuple key)
        {
            return new TypeAdapterRule
            {
                Priority = arg =>
                {
                    var score1 = GetSubclassDistance(arg.DestinationType, key.Destination, _AllowImplicitDestinationInheritance);
                    if (score1 == null)
                        return null;
                    var score2 = GetSubclassDistance(arg.SourceType, key.Source, _AllowImplicitSourceInheritance);
                    if (score2 == null)
                        return null;
                    return score1.Value + score2.Value;
                },
                Settings = new TypeAdapterSettings(),
            };
        }

        private static int? GetSubclassDistance(Type type1, Type type2, bool allowInheritance)
        {
            //Support for using ValueType mapping configurations of types, for mapping cases on Nulllable ValueType values
            if (type2.IsInterface && type1.IsNullable() && !type1.ContainsGenericParameters)
                type1 = type1.GetGenericArguments().FirstOrDefault();

            if (type1 == type2)
                return 50;

            //generic type definition
            int score = 35;
            if (type2.GetTypeInfo().IsGenericTypeDefinition)
            {
                while (type1 != null && type1.GetTypeInfo().IsGenericType && type1.GetGenericTypeDefinition() != type2)
                {
                    score--;
                    type1 = type1.GetTypeInfo().BaseType;
                }
                return type1 != null && type1.GetTypeInfo().IsGenericType && type1.GetGenericTypeDefinition() == type2
                    ? (int?)score
                    : null;
            }
            if (!allowInheritance)
                return null;

            if (!type2.GetTypeInfo().IsAssignableFrom(type1.GetTypeInfo()))
                return null;

            //interface
            if (type2.GetTypeInfo().IsInterface)
                return 25;

            //base type
            score = 50;
            while (type1 != null && type1 != type2)
            {
                score--;
                type1 = type1.GetTypeInfo().BaseType;
            }
            return score;
        }

        private TypeAdapterSettings GetSettings(TypeTuple types)
        {
            RuleMap.TryGetValue(types, out var ruleMap);

            if (ruleMap == null)
            {
                ruleMap = types.Source == typeof(void)
                ? CreateDestinationTypeRule(types)
                : CreateTypeTupleRule(types);

                ruleMap.Settings.Id = ++_Id;
                Rules.Add(ruleMap);
                RuleMap[types] = ruleMap;
            }
            else
                ruleMap.Settings.Version += 1;

            return ruleMap.Settings;
        }

        private void Remove(TypeTuple key)
        {
            RuleMap.TryGetValue(key, out var rule);
            if(rule != null)
            {
                Rules.Remove(rule);
                RuleMap.Remove(key);
            }
            _mapCache.Remove(key);
            _mapToTargetCache.Remove(key);
            _ProjectionCache.Remove(key);
            _dynamicMapCache.Remove(key);
        }

        public SettingsCacheStorage(IList<TypeAdapterRule> ruleTemplates, TypeAdapterConfig config)
        {
            ImmutableRules = new List<TypeAdapterRule>(ruleTemplates);
            var setting = TypeAdapterSettings.CreateDefault;
            DefaultSettings = setting;
            DefaultSetter = new DefaultOnlySetter(setting, config);
        }

        public DefaultOnlySetter GetDefaultSetter()
        {
            lock (_lock)
            {
                DefaultSettings.Version = ++DefaultSettings.Version;
                return DefaultSetter;
            }
        }

        public void AllowImplicitDestinationInheritance(bool value)
        {
            lock (_lock)
            {
                _AllowImplicitDestinationInheritance = value;
            }
        }
        public void AllowImplicitSourceInheritance(bool value)
        {
            lock (_lock)
            {
                _AllowImplicitSourceInheritance = value;
            }
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
        public void AddAdapterRule(BaseAdapter adapter)
        {
            lock (_lock)
            {
                ImmutableRules.Add(adapter.CreateRule());
            }
        }

        public void AddImmutableSettingsRule(TypeAdapterRule rule)
        {
            lock (_lock)
            {
                ImmutableRules.Add(rule);
            }
        }

        public void AddRule(TypeAdapterRule rule)
        {
            lock (_lock)
            {
                Rules.Add(rule);
            }
        }
    }
}
