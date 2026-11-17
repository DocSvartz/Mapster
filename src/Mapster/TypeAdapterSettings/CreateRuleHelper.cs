using Mapster.Models;
using System;

namespace Mapster.TypeAdapterSettings
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
    }
}
