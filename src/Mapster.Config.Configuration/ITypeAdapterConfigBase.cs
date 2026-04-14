using System;

namespace Mapster.Configuration
{
    public interface ITypeAdapterConfigBase
    {
        bool AllowImplicitDestinationInheritance { get; set; }
        bool AllowImplicitSourceInheritance { get; set; }
        TypeAdapterSetter Default { get; }
        bool RequireDestinationMemberSource { get; set; }
        bool RequireExplicitMapping { get; set; }
        bool RequireExplicitMappingPrimitive { get; set; }
        TypeAdapterSetter NewConfig(Type sourceType, Type destinationType);
        TypeAdapterSetter<TSource, TDestination> NewConfig<TSource, TDestination>();
        TypeAdapterSetter ForDestinationType(Type destinationType);
        TypeAdapterSetter<TDestination> ForDestinationType<TDestination>();
        TypeAdapterSetter ForType(Type sourceType, Type destinationType);
        TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>();
    }
}