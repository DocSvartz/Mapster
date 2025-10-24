using Mapster.Models;
using Mapster.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Mapster
{
    public static class TypeAdapterConfigSettingsExtentions
    {
        private static int? GetSubclassDistance(Type type1, Type type2, bool allowInheritance)
        {
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

        private static TypeAdapterRule CreateDestinationTypeRule(TypeTuple key)
        {
            return new TypeAdapterRule
            {
                Priority = arg => GetSubclassDistance(arg.DestinationType, key.Destination, true),
                Settings = new TypeAdapterSettings(),
            };
        }

        private static TypeAdapterRule CreateTypeTupleRule(this ITypeAdapterConfig config, TypeTuple key)
        {
            return new TypeAdapterRule
            {
                Priority = arg =>
                {
                    var score1 = GetSubclassDistance(arg.DestinationType, key.Destination, config.AllowImplicitDestinationInheritance);
                    if (score1 == null)
                        return null;
                    var score2 = GetSubclassDistance(arg.SourceType, key.Source, config.AllowImplicitSourceInheritance);
                    if (score2 == null)
                        return null;
                    return score1.Value + score2.Value;
                },
                Settings = new TypeAdapterSettings(),
            };
        }

        private static TypeAdapterSettings CreateSettings(this ITypeAdapterConfig config, BaseAdaptAttribute attr)
        {
            var settings = new TypeAdapterSettings();
            var setter = new TypeAdapterSetter(settings, config);
            setter.ApplyAdaptAttribute(attr);
            return settings;
        }


        public static TypeAdapterSettings GetSettings(this ITypeAdapterConfig config, TypeTuple key)
        {
            var rule = config.RuleMap.GetOrAdd(key, types =>
            {
                var r = types.Source == typeof(void)
                    ? CreateDestinationTypeRule(types)
                    : config.CreateTypeTupleRule(types);
                config.Rules.LockAdd(r);
                return r;
            });

            rule.Settings.SourceType = key.Source;
            rule.Settings.DestinationType = key.Destination;

            return rule.Settings;
        }

        private static IEnumerable<TypeAdapterRule> GetAttributeSettings(this ITypeAdapterConfig config, TypeTuple tuple, MapType mapType)
        {
            var rules1 = from type in tuple.Source.GetAllTypes()
                         from o in type.GetTypeInfo().GetCustomAttributesData()
                         where typeof(AdaptToAttribute).IsAssignableFrom(o.GetAttributeType())
                         let attr = o.CreateCustomAttribute<AdaptToAttribute>()
                         where attr != null && (attr.MapType & mapType) != 0
                         where attr.Type == null || attr.Type == tuple.Destination
                         where attr.Name == null || attr.Name.Replace("[name]", type.Name) == tuple.Destination.Name
                         let distance = GetSubclassDistance(tuple.Source, type, true)
                         select new TypeAdapterRule
                         {
                             Priority = arg => distance + 50,
                             Settings = config.CreateSettings(attr)
                         };
            if (tuple.Source == tuple.Destination)
                return rules1;
            var rules2 = from type in tuple.Destination.GetAllTypes()
                         from o in type.GetTypeInfo().GetCustomAttributesData()
                         where typeof(AdaptFromAttribute).IsAssignableFrom(o.GetAttributeType()) ||
                               typeof(AdaptTwoWaysAttribute).IsAssignableFrom(o.GetAttributeType())
                         let attr = o.CreateCustomAttribute<BaseAdaptAttribute>()
                         where attr != null && (attr.MapType & mapType) != 0
                         where attr.Type == null || attr.Type == tuple.Source
                         where attr.Name == null || attr.Name.Replace("[name]", type.Name) == tuple.Source.Name
                         let distance = GetSubclassDistance(tuple.Destination, type, true)
                         select new TypeAdapterRule
                         {
                             Priority = arg => distance + 50,
                             Settings = config.CreateSettings(attr)
                         };
            return rules1.Concat(rules2);
        }




        internal static TypeAdapterSettings GetMergedSettings(this ITypeAdapterConfig config,  TypeTuple tuple, MapType mapType)
        {
            var arg = new PreCompileArgument
            {
                SourceType = tuple.Source,
                DestinationType = tuple.Destination,
                MapType = mapType,
                ExplicitMapping = config.RuleMap.ContainsKey(tuple),
            };

            //auto add setting if there is attr setting
            var attrSettings = config.GetAttributeSettings(tuple, mapType).ToList();
            if (!arg.ExplicitMapping && attrSettings.Any(rule => rule.Priority(arg) == 100))
            {
                config.GetSettings(tuple);
                arg.ExplicitMapping = true;
            }

            var result = new TypeAdapterSettings();
            lock (config.Rules)
            {
                var rules = config.Rules.Reverse<TypeAdapterRule>().Concat(attrSettings);
                var settings = from rule in rules
                               let priority = rule.Priority(arg)
                               where priority != null
                               orderby priority.Value descending
                               select rule.Settings;
                foreach (var setting in settings)
                {
                    result.Apply(setting);
                }
            }

            //remove recursive include types
            if (mapType == MapType.MapToTarget)
                result.Includes.Remove(tuple);
            else
                result.Includes.RemoveAll(t => t.Source == tuple.Source);
            return result;
        }
    }
}
