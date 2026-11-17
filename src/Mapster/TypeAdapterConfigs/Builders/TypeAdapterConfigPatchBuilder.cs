using Mapster.TypeAdapterSetters;
using System;
using System.Linq.Expressions;

namespace Mapster.TypeAdapterConfigs.Builders
{
    public class TypeAdapterConfigPatchBuilder : ConfigBuilderBase
    {
        public TypeAdapterConfigPatchBuilder() {}

        public static TypeAdapterConfigPatchBuilder Create() => new();

        public TypeAdapterConfigPatchBuilder NewConfig<TSource, TDestination>(
            Action<TypeAdapterSetter<TSource, TDestination>> setup)
            => this.RegisterNewConfig<TSource, TDestination, TypeAdapterConfigPatchBuilder>(setup);

        public TypeAdapterConfigPatchBuilder NewConfig(
            Type sourceType,
            Type destinationType,
            Action<TypeAdapterSetter> setup)
            => this.RegisterNewConfig<TypeAdapterConfigPatchBuilder>(sourceType, destinationType, setup);

        // ---- ForType family ----

        public TypeAdapterConfigPatchBuilder ForDestinationType<TDestination>(
            Action<TypeAdapterSetter<TDestination>> setup)
            => this.RegisterForDestinationType<TDestination, TypeAdapterConfigPatchBuilder>(setup);

        public TypeAdapterConfigPatchBuilder ForDestinationType(
            Type destinationType,
            Action<DestinationTypeOnlySetter> setup)
            => this.RegisterForDestinationType<TypeAdapterConfigPatchBuilder>(destinationType, setup);

        public TypeAdapterConfigPatchBuilder ForType<TSource, TDestination>(
            Action<TypeAdapterSetter<TSource, TDestination>> setup)
            => this.RegisterForType<TSource, TDestination, TypeAdapterConfigPatchBuilder>(setup);

        public TypeAdapterConfigPatchBuilder ForType(
            Type sourceType,
            Type destinationType,
            Action<TypeAdapterSetter> setup)
            => this.RegisterForType<TypeAdapterConfigPatchBuilder>(sourceType, destinationType, setup);

        // ---- TwoWays ----

        public TypeAdapterConfigPatchBuilder AddTwoWaysMapping<TSource, TDestination>(
            Action<TwoWaysTypeAdapterSetter<TSource, TDestination>> setup)
            => this.RegisterTwoWaysMapping<TSource, TDestination, TypeAdapterConfigPatchBuilder>(setup);

        public TypeAdapterConfigPatchBuilder AddTwoWaysMapping<TSource, TDestination>(
            Action<TypeAdapterSetter<TSource, TDestination>> forwardSetup,
            Action<TwoWaysTypeAdapterSetter<TSource, TDestination>> twoWaysSetup)
            => this.RegisterTwoWaysMapping<TSource, TDestination, TypeAdapterConfigPatchBuilder>(forwardSetup, twoWaysSetup);

        // ---- When ----

        public TypeAdapterConfigPatchBuilder When(
            string description,
            Func<Type, Type, MapType, bool> canMap,
            Action<TypeAdapterSetter> setup)
            => this.RegisterWhen(description, canMap, setup);

        public TypeAdapterConfigPatchBuilder When(
            string description,
            Func<PreCompileArgument, bool> canMap,
            Action<TypeAdapterSetter> setup)
            => this.RegisterWhen(description, canMap, setup);

        // ---- Default ----

        public TypeAdapterConfigPatchBuilder AddDefault(Action<DefaultOnlySetter> setup)
            => this.RegisterDefault(setup);

        // ---- Settings ----

        public TypeAdapterConfigPatchBuilder ActivateCustomConvertersSrcNullPropagation(bool value)
            => this.RegisterActivateCustomConvertersSrcNullPropagation(value);

        public TypeAdapterConfigPatchBuilder AllowImplicitDestinationInheritance(bool value)
            => this.RegisterAllowImplicitDestinationInheritance(value);

        public TypeAdapterConfigPatchBuilder AllowImplicitSourceInheritance(bool value)
            => this.RegisterAllowImplicitSourceInheritance(value);

        public TypeAdapterConfigPatchBuilder RequireExplicitMapping(bool value)
            => this.RegisterRequireExplicitMapping(value);

        public TypeAdapterConfigPatchBuilder RequireExplicitMappingPrimitive(bool value)
            => this.RegisterRequireExplicitMappingPrimitive(value);

        public TypeAdapterConfigPatchBuilder RequireDestinationMemberSource(bool value)
            => this.RegisterRequireDestinationMemberSource(value);

        public TypeAdapterConfigPatchBuilder SelfContainedCodeGeneration(bool value)
            => this.RegisterSelfContainedCodeGeneration(value);

        public TypeAdapterConfigPatchBuilder Compiler(
            string description,
            Func<LambdaExpression, Delegate> compiler)
            => this.RegisterCompiler(description, compiler);

    }
}
