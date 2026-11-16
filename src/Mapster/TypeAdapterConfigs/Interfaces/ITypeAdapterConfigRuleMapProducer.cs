using Mapster.TypeAdapterSetters;
using System;

namespace Mapster
{
    public interface ITypeAdapterConfigRuleMapProducer
    {
        DestinationTypeOnlySetter ForDestinationType(Type destinationType);
        TypeAdapterSetter<TDestination> ForDestinationType<TDestination>();
        TypeAdapterSetter ForType(Type sourceType, Type destinationType);
        TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>();
        TypeAdapterSetter NewConfig(Type sourceType, Type destinationType);
        TypeAdapterSetter<TSource, TDestination> NewConfig<TSource, TDestination>();
    }
}
