using Mapster.Adapters;
using Mapster.TypeAdapterSetters;
using System;
using System.Linq.Expressions;

namespace Mapster.TypeAdapterConfigs.Builders
{
    public class TypeAdapterConfigInitBuilder : ConfigBuilderBase
    {
        public TypeAdapterConfigInitBuilder() {}

        public static TypeAdapterConfigInitBuilder Create() => new();
                

        public TypeAdapterConfigInitBuilder NewConfig<TSource, TDestination>(
            Action<TypeAdapterSetter<TSource, TDestination>> setup)
            => this.RegisterNewConfig<TSource, TDestination, TypeAdapterConfigInitBuilder>(setup);

        public TypeAdapterConfigInitBuilder NewConfig(
            Type sourceType,
            Type destinationType,
            Action<TypeAdapterSetter> setup)
            => this.RegisterNewConfig<TypeAdapterConfigInitBuilder>(sourceType, destinationType, setup);

        // ---- ForType family ----

        public TypeAdapterConfigInitBuilder ForDestinationType<TDestination>(
            Action<TypeAdapterSetter<TDestination>> setup)
            => this.RegisterForDestinationType<TDestination, TypeAdapterConfigInitBuilder>(setup);

        public TypeAdapterConfigInitBuilder ForDestinationType(
            Type destinationType,
            Action<DestinationTypeOnlySetter> setup)
            => this.RegisterForDestinationType<TypeAdapterConfigInitBuilder>(destinationType, setup);

        // ---- TwoWays ----

        public TypeAdapterConfigInitBuilder TwoWays<TSource, TDestination>(
            Action<TwoWaysTypeAdapterSetter<TSource, TDestination>> setup)
            => this.RegisterTwoWaysMapping<TSource, TDestination, TypeAdapterConfigInitBuilder>(setup);

        public TypeAdapterConfigInitBuilder TwoWays<TSource, TDestination>(
            Action<TypeAdapterSetter<TSource, TDestination>> forwardSetup,
            Action<TwoWaysTypeAdapterSetter<TSource, TDestination>> twoWaysSetup)
            => this.RegisterTwoWaysMapping<TSource, TDestination, TypeAdapterConfigInitBuilder>(forwardSetup, twoWaysSetup);

        // ---- When ----

        public TypeAdapterConfigInitBuilder When(
            Func<Type, Type, MapType, bool> canMap,
            Action<TypeAdapterSetter>? setup = null,
            string description = "When Func<Type, Type, MapType, bool> Step")
            => this.RegisterWhen(description, canMap, setup ?? WhenSetupActionEmpty);

        public TypeAdapterConfigInitBuilder When(
            Func<PreCompileArgument, bool> canMap,
            Action<TypeAdapterSetter>? setup = null,
            string description = "When Func<PreCompileArgument, bool> Step")
            => this.RegisterWhen(description, canMap, setup ?? WhenSetupActionEmpty);

        // ---- Default ----

        public TypeAdapterConfigInitBuilder Default(Action<DefaultOnlySetter> setup)
            => this.RegisterDefault(setup);

        // ---- Adapters ----

        public TypeAdapterConfigInitBuilder AddAdapter<TAdapter>(TAdapter adapter)
            where TAdapter : BaseAdapter
            => this.RegisterAddAdapter<TAdapter, TypeAdapterConfigInitBuilder>(adapter);

        public TypeAdapterConfigInitBuilder AddAdapter<TAdapter>()
            where TAdapter : BaseAdapter, new()
            => this.RegisterAddAdapter<TAdapter, TypeAdapterConfigInitBuilder>();

        // ---- Settings ----

        public TypeAdapterConfigInitBuilder ActivateCustomConvertersSrcNullPropagation(bool value)
            => this.RegisterActivateCustomConvertersSrcNullPropagation(value);

        public TypeAdapterConfigInitBuilder AllowImplicitDestinationInheritance(bool value)
            => this.RegisterAllowImplicitDestinationInheritance(value);

        public TypeAdapterConfigInitBuilder AllowImplicitSourceInheritance(bool value)
            => this.RegisterAllowImplicitSourceInheritance(value);

        public TypeAdapterConfigInitBuilder RequireExplicitMapping(bool value)
            => this.RegisterRequireExplicitMapping(value);

        public TypeAdapterConfigInitBuilder RequireExplicitMappingPrimitive(bool value)
            => this.RegisterRequireExplicitMappingPrimitive(value);

        public TypeAdapterConfigInitBuilder RequireDestinationMemberSource(bool value)
            => this.RegisterRequireDestinationMemberSource(value);

        public TypeAdapterConfigInitBuilder SelfContainedCodeGeneration(bool value)
            => this.RegisterSelfContainedCodeGeneration(value);

        public TypeAdapterConfigInitBuilder Compiler(
            string description,
            Func<LambdaExpression, Delegate> compiler)
            => this.RegisterCompiler(description, compiler);

    }
}
