using Mapster.TypeAdapterSetters;
using System;

namespace Mapster.TypeAdapterConfigs.Builders.Interfaces
{
    public interface ITypeAdapterConfigRuleMapBuildOnlyProducer
    {
        DestinationTypeOnlySetter ForDestinationType(Type destinationType);
        TypeAdapterSetter<TDestination> ForDestinationType<TDestination>();
        TypeAdapterSetter NewConfig(Type sourceType, Type destinationType);
        TypeAdapterSetter<TSource, TDestination> NewConfig<TSource, TDestination>();
    }
}
