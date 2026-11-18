using Mapster.Adapters;
using Mapster.Cache;
using Mapster.TypeAdapterSetters;
using System;
using System.Linq.Expressions;

namespace Mapster.TypeAdapterConfigs.Builders
{
    /// <summary>
    /// Shared implementation of all builder steps.
    /// Each step exists here exactly ONCE.
    /// Concrete builders expose thin wrappers that delegate to these methods;
    /// which wrappers a builder declares defines which steps are available.
    /// Internal: not visible outside the assembly, only wrappers are public.
    /// </summary>
    internal static class ConfigBuilderStepsCore
    {
        // ---- NewConfig family ----

        /// <summary>
        /// Adds a NEW mapping rule for a type pair.
        /// </summary>
        internal static TSelf RegisterNewConfig<TSource, TDestination, TSelf>(
            this TSelf self,
            Action<TypeAdapterSetter<TSource, TDestination>> setup)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterMappingStep(
                MappingKind.NewConfig,
                $"NewConfig<{typeof(TSource).Name}, {typeof(TDestination).Name}>",
                typeof(TSource),
                typeof(TDestination),
                twoWays: false,
                cfg =>
                {
                    var setter = cfg.NewConfig<TSource, TDestination>();
                    setup(setter);
                });
            return self;
        }

        /// <summary>
        /// Same as the generic overload but for types known only at runtime.
        /// Uses the same NewConfig key, so generic and non-generic
        /// registrations for one pair still collide — as intended.
        /// </summary>
        internal static TSelf RegisterNewConfig<TSelf>(
            this TSelf self,
            Type sourceType,
            Type destinationType,
            Action<TypeAdapterSetter> setup)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterMappingStep(
                MappingKind.NewConfig,
                $"NewConfig({sourceType.Name} -> {destinationType.Name})",
                sourceType,
                destinationType,
                twoWays: false,
                cfg =>
                {
                    var setter = cfg.NewConfig(sourceType, destinationType);
                    setup(setter);
                });
            return self;
        }

        // ---- ForType family ----

        /// <summary>
        /// Adds a DESTINATION-ONLY rule: applies to all types assignable
        /// to TDestination. Keyed in its own namespace ("Destination:..."),
        /// so it never collides with pair-based mappings.
        /// </summary>
        internal static TSelf RegisterForDestinationType<TDestination, TSelf>(
            this TSelf self,
            Action<TypeAdapterSetter<TDestination>> setup)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterMappingStep(
                MappingKind.DestinationType,
                $"ForDestinationType<{typeof(TDestination).Name}>",
                sourceType: null,
                typeof(TDestination),
                twoWays: false,
                cfg =>
                {
                    var setter = cfg.ForDestinationType<TDestination>();
                    setup(setter);
                });
            return self;
        }

        /// <summary>
        /// Adds a DESTINATION-ONLY rule when the destination type
        /// is known only at runtime (non-generic overload).
        /// Rules are set by member names, not by lambdas.
        /// </summary>
        internal static TSelf RegisterForDestinationType<TSelf>(
            this TSelf self,
            Type destinationType,
            Action<DestinationTypeOnlySetter> setup)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterMappingStep(
                MappingKind.DestinationType,
                $"ForDestinationType({destinationType.Name})",
                sourceType: null,          // no source type for destination-only rules
                destinationType,
                twoWays: false,
                cfg =>
                {
                    var setter = cfg.ForDestinationType(destinationType);
                    setup(setter);
                });
            return self;
        }

        /// <summary>
        /// Adds a FOR-TYPE rule: enhances the existing mapping for the pair.
        /// Uses the same pair key as NewConfig, so ForType on top of NewConfig
        /// for the same pair is rejected — as intended.
        /// </summary>
        internal static TSelf RegisterForType<TSource, TDestination, TSelf>(
            this TSelf self,
            Action<TypeAdapterSetter<TSource, TDestination>> setup)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterMappingStep(
                MappingKind.ForType,
                $"ForType<{typeof(TSource).Name}, {typeof(TDestination).Name}>",
                typeof(TSource),
                typeof(TDestination),
                twoWays: false,
                cfg =>
                {
                    var setter = cfg.ForType<TSource, TDestination>();
                    setup(setter);
                });
            return self;
        }

        /// <summary>
        /// Same as the generic overload but for types known only at runtime.
        /// Uses the same pair key, so generic and non-generic ForType
        /// registrations for one pair collide — as intended.
        /// </summary>
        internal static TSelf RegisterForType<TSelf>(
            this TSelf self,
            Type sourceType,
            Type destinationType,
            Action<TypeAdapterSetter> setup)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterMappingStep(
                MappingKind.ForType,
                $"ForType({sourceType.Name} -> {destinationType.Name})",
                sourceType,
                destinationType,
                twoWays: false,
                cfg =>
                {
                    var setter = cfg.ForType(sourceType, destinationType);
                    setup(setter);
                });
            return self;
        }


        // ---- TwoWays ----

        /// <summary>
        /// Adds a TWO-WAYS mapping: every rule applies to BOTH directions.
        /// Claims both directions in the pair registry: a later registration
        /// for the same pair in either direction is rejected.
        /// </summary>
        internal static TSelf RegisterTwoWaysMapping<TSource, TDestination, TSelf>(
            this TSelf self,
            Action<TwoWaysTypeAdapterSetter<TSource, TDestination>> setup)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterMappingStep(
                MappingKind.NewConfig,
                $"NewConfig<{typeof(TSource).Name}, {typeof(TDestination).Name}> (TwoWays)",
                typeof(TSource),
                typeof(TDestination),
                twoWays: true, // claims both directions in the registry
                cfg =>
                {
                    var twoWaysSetter = cfg.NewConfig<TSource, TDestination>().TwoWays();
                    setup(twoWaysSetter);
                });
            return self;
        }

        /// <summary>
        /// Adds a TWO-WAYS mapping with direction-specific forward rules.
        /// forwardSetup runs on the plain setter BEFORE TwoWays(),
        /// so those rules apply ONLY to the forward direction.
        /// twoWaysSetup runs AFTER TwoWays(), rules apply to BOTH directions.
        /// </summary>
        internal static TSelf RegisterTwoWaysMapping<TSource, TDestination, TSelf>(
            this TSelf self,
            Action<TypeAdapterSetter<TSource, TDestination>> forwardSetup,
            Action<TwoWaysTypeAdapterSetter<TSource, TDestination>> twoWaysSetup)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterMappingStep(
                MappingKind.NewConfig,
                $"NewConfig<{typeof(TSource).Name}, {typeof(TDestination).Name}> (Forward + TwoWays)",
                typeof(TSource),
                typeof(TDestination),
                twoWays: true,
                cfg =>
                {
                    // Order matters: forward-only rules must be set before
                    // TwoWays() is invoked, because TwoWays() returns a NEW
                    // wrapper setter and does not capture earlier rules.
                    var setter = cfg.NewConfig<TSource, TDestination>();
                    forwardSetup?.Invoke(setter);
                    twoWaysSetup?.Invoke(setter.TwoWays());
                });
            return self;
        }

        // ---- When (conditional rules; coexist, no pair-key guard) ----

        /// <summary>
        /// Adds a CONDITIONAL rule based on (source, dest, MapType).
        /// </summary>
        internal static TSelf RegisterWhen<TSelf>(
            this TSelf self,
            string description,
            Func<Type, Type, MapType, bool> canMap,
            Action<TypeAdapterSetter> setup)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterStep(
                $"When({description})",
                cfg =>
                {
                    var setter = cfg.When(canMap);
                    setup(setter);
                });
            return self;
        }

        /// <summary>
        /// Adds a CONDITIONAL rule based on PreCompileArgument.
        /// Richer context: member expressions and attributes are available
        /// inside the predicate. The predicate is evaluated per member
        /// at rule-compile time, not on every Adapt call.
        /// </summary>
        internal static TSelf RegisterWhen<TSelf>(
            this TSelf self,
            string description,
            Func<PreCompileArgument, bool> canMap,
            Action<TypeAdapterSetter> setup)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterStep(
                $"When(PreCompileArgument: {description})",
                cfg =>
                {
                    // When returns a TypeAdapterSetter, so the user setup
                    // can chain Map/Ignore/AfterMapping etc. on top of it.
                    var setter = cfg.When(canMap);
                    setup(setter);
                });
            return self;
        }

        // ---- Default ----

        /// <summary>
        /// Adds a DEFAULT settings rule — the fallback block used for type pairs
        /// that have no explicit NewConfig/ForType rule.
        /// Registered in the pair registry under its own single key,
        /// so a second call for Default is rejected immediately.
        /// </summary>
        internal static TSelf RegisterDefault<TSelf>(
            this TSelf self,
            Action<DefaultOnlySetter> setup)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterMappingStep(
                MappingKind.DefaultSetter,
                "Default",
                typeof(MapsterCacheMarker),
                typeof(DefaultOnlySetter),
                twoWays: false,
                cfg =>
                {
                    var setter = cfg.Default;
                    setup(setter);
                });
            return self;
        }

        // ---- Settings: boolean properties ----

        /// <summary>
        /// Registers a boolean config property via the property guard.
        /// Shared by all boolean wrappers to avoid repeating plumbing.
        /// </summary>
        internal static TSelf RegisterBoolProperty<TSelf>(
            this TSelf self,
            string propertyKey,
            bool value,
            Action<BuilderTypeAdapterConfig, bool> apply)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterPropertyStep(propertyKey, value.ToString(), value, apply);
            return self;
        }

        internal static TSelf RegisterActivateCustomConvertersSrcNullPropagation<TSelf>(
            this TSelf self, bool value)
            where TSelf : IConfigBuilderStepsCore
            => self.RegisterBoolProperty(
                nameof(TypeAdapterConfig.ActivateCustomConvertersSrcNullPropagation),
                value,
                (cfg, v) => cfg.ActivateCustomConvertersSrcNullPropagation = v);

        internal static TSelf RegisterAllowImplicitDestinationInheritance<TSelf>(
            this TSelf self, bool value)
            where TSelf : IConfigBuilderStepsCore
            => self.RegisterBoolProperty(
                nameof(TypeAdapterConfig.AllowImplicitDestinationInheritance),
                value,
                (cfg, v) => cfg.AllowImplicitDestinationInheritance = v);

        internal static TSelf RegisterAllowImplicitSourceInheritance<TSelf>(
            this TSelf self, bool value)
            where TSelf : IConfigBuilderStepsCore
            => self.RegisterBoolProperty(
                nameof(TypeAdapterConfig.AllowImplicitSourceInheritance),
                value,
                (cfg, v) => cfg.AllowImplicitSourceInheritance = v);

        internal static TSelf RegisterRequireExplicitMapping<TSelf>(
            this TSelf self, bool value)
            where TSelf : IConfigBuilderStepsCore
            => self.RegisterBoolProperty(
                nameof(TypeAdapterConfig.RequireExplicitMapping),
                value,
                (cfg, v) => cfg.RequireExplicitMapping = v);

        internal static TSelf RegisterRequireExplicitMappingPrimitive<TSelf>(
            this TSelf self, bool value)
            where TSelf : IConfigBuilderStepsCore
            => self.RegisterBoolProperty(
                nameof(TypeAdapterConfig.RequireExplicitMappingPrimitive),
                value,
                (cfg, v) => cfg.RequireExplicitMappingPrimitive = v);

        internal static TSelf RegisterRequireDestinationMemberSource<TSelf>(
            this TSelf self, bool value)
            where TSelf : IConfigBuilderStepsCore
            => self.RegisterBoolProperty(
                nameof(TypeAdapterConfig.RequireDestinationMemberSource),
                value,
                (cfg, v) => cfg.RequireDestinationMemberSource = v);

        internal static TSelf RegisterSelfContainedCodeGeneration<TSelf>(
            this TSelf self, bool value)
            where TSelf : IConfigBuilderStepsCore
            => self.RegisterBoolProperty(
                nameof(TypeAdapterConfig.SelfContainedCodeGeneration),
                value,
                (cfg, v) => cfg.SelfContainedCodeGeneration = v);

        // ---- Special (non-boolean) settings ----

        /// <summary>
        /// Overrides the compiler used to turn lambda expressions into delegates.
        /// Name is descriptive only — a delegate value cannot be meaningfully printed.
        /// </summary>
        internal static TSelf RegisterCompiler<TSelf>(
            this TSelf self,
            string description,
            Func<LambdaExpression, Delegate> compiler)
            where TSelf : IConfigBuilderStepsCore
        {
            self.RegisterStep($"Compiler({description})", cfg => cfg.Compiler = compiler);
            return self;
        }

        // ---- Adapters ----

        /// <summary>
        /// Adds an ADAPTER step. Uniqueness is by the adapter TYPE:
        /// the same adapter type cannot be added twice, even with
        /// different instances. The instance is captured and the rule
        /// is created at Build() time.
        /// </summary>
        internal static TSelf RegisterAddAdapter<TAdapter, TSelf>(
            this TSelf self,
            TAdapter adapter)
            where TSelf : IConfigBuilderStepsCore
            where TAdapter : BaseAdapter
        {
            self.RegisterAdapterStep(
                $"AddAdapter<{typeof(TAdapter).Name}>",
                () => adapter,
                (cfg, a) => cfg.AddAdapter(a));
            return self;
        }

        /// <summary>
        /// Same as the instance overload, but the adapter is created lazily
        /// inside Build() via its parameterless constructor.
        /// Fits the deferred-registration philosophy: nothing runs
        /// until Build() is called.
        /// </summary>
        internal static TSelf RegisterAddAdapter<TAdapter, TSelf>(
            this TSelf self)
            where TSelf : IConfigBuilderStepsCore
            where TAdapter : BaseAdapter, new()
        {
            self.RegisterAdapterStep(
                $"AddAdapter<{typeof(TAdapter).Name}> (deferred)",
                () => new TAdapter(),
                (cfg, a) => cfg.AddAdapter(a));
            return self;
        }


    }
}
