using Mapster.TypeAdapterSetters;
using System;
using System.Linq.Expressions;

namespace Mapster
{
    public class FrozenTypeAdapterConfig :
        ITypeAdapterConfigSettings,
        ITypeAdapterConfigRuleMapProducer,
        ITypeAdapterConfigCustomRuleProducer,
        ITypeAdapterConfigAdapterConsumer
    {
        public bool ActivateCustomConvertersSrcNullPropagation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AllowImplicitDestinationInheritance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AllowImplicitSourceInheritance { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool RequireDestinationMemberSource { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool RequireExplicitMapping { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool RequireExplicitMappingPrimitive { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool SelfContainedCodeGeneration { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Func<LambdaExpression, Delegate> Compiler { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public DefaultOnlySetter Default => throw new NotImplementedException();

        public DestinationTypeOnlySetter ForDestinationType(Type destinationType)
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

        public TypeAdapterSetter When(Func<PreCompileArgument, bool> canMap)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter When(Func<Type, Type, MapType, bool> canMap)
        {
            throw new NotImplementedException();
        }
    }
}
