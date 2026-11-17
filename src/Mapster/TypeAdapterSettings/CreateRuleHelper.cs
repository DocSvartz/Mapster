using Mapster.Cache;
using Mapster.Models;
using Mapster.TypeAdapterSetters;
using System;

namespace Mapster
{
    internal static class CreateRuleHelper
    {
        internal static TypeAdapterRule CreateDestinationTypeRule(TypeTuple key, TypeAdapterSettings? settings = null)
        {
            Func<PreCompileArgument, int?> priority = arg => arg.DestinationType.GetSubclassDistance(key.Destination, true);

            return settings is null
                ? new TypeAdapterRule(priority)
                : new TypeAdapterRule(priority, settings);
        }

        internal static TypeAdapterRule CreateDefaultRule(TypeAdapterSettings? settings = null)
        {
            Func<PreCompileArgument, int?> priority = arg => -100;

            return settings is null
                ? new TypeAdapterRule(priority)
                : new TypeAdapterRule(priority, settings);
        }

        internal static TypeAdapterRule CreateTypeTupleRule(TypeTuple key,
            bool allowImplicitDestinationInheritance,
            bool allowImplicitSourceInheritance,
            TypeAdapterSettings? settings = null)
        {
            Func<PreCompileArgument, int?> priority =
                arg =>
                {
                    var score1 = arg.DestinationType.GetSubclassDistance(key.Destination, allowImplicitDestinationInheritance);
                    if (score1 == null)
                        return null;
                    var score2 = arg.SourceType.GetSubclassDistance(key.Source, allowImplicitSourceInheritance);
                    if (score2 == null)
                        return null;
                    return score1.Value + score2.Value;
                };

            return settings is null
                ? new TypeAdapterRule(priority)
                : new TypeAdapterRule(priority, settings);
        }

        internal static TypeAdapterRule CreateWhenRule(Func<Type, Type, MapType, bool> canMap)
        {
            var rule = new TypeAdapterRule
            {
                Priority = arg => canMap(arg.SourceType, arg.DestinationType, arg.MapType) ? (int?)25 : null
            };

            return rule;
        }

        internal static TypeAdapterRule CreateWhenRule(Func<PreCompileArgument, bool> canMap)
        {
            var rule = new TypeAdapterRule
            {
                Priority = arg => canMap(arg) ? (int?)25 : null
            };

            return rule;
        }

        internal static TypeAdapterRule CreateRuleForTypeConfigBuilder(TypeTuple types, ITypeAdapterConfigSettings settings)
        {
            if (types.Source == typeof(MapsterCacheMarker)
                && types.Destination == typeof(DefaultOnlySetter))
                return CreateDefaultRule();

            else if (types.Source == typeof(void))
                return CreateDestinationTypeRule(types);
            else
                return CreateTypeTupleRule(types,
                    settings.AllowImplicitDestinationInheritance,
                    settings.AllowImplicitSourceInheritance);
        }
    }
}
