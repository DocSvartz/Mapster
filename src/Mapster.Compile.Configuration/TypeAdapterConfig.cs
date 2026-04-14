using Mapster.Configuration;
using System;

namespace Mapster.Compile.Configuration
{
    public class TypeAdapterConfig : ITypeAdapterConfigBase
    {
        public bool AllowImplicitDestinationInheritance { get; set; }
        public bool AllowImplicitSourceInheritance { get; set; }
        public TypeAdapterSetter Default => throw new NotImplementedException();
        public bool RequireDestinationMemberSource { get; set; }
        public bool RequireExplicitMapping { get; set; }
        public bool RequireExplicitMappingPrimitive { get; set; }

        public TypeAdapterSetter ForDestinationType(Type destinationType)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter<TDestination> ForDestinationType<TDestination>()
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter ForType(Type sourceType, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>()
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter NewConfig(Type sourceType, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter<TSource, TDestination> NewConfig<TSource, TDestination>()
        {
            throw new NotImplementedException();
        }
    }
}
