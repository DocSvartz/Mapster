using Mapster.Cache;
using Mapster.TypeAdapterSetters;
using System;
using System.Linq.Expressions;

namespace Mapster
{
    public sealed partial class TypeAdapterConfigBuilder
    {
        #region TypeAdapterSetterSettings
       
        /// <summary>
        /// Adds a DEFAULT settings rule — the fallback block used for type pairs
        /// that have no explicit NewConfig/ForType rule.
        /// Treated as a mapping-like rule (own MappingKind), not a property:
        /// registered in the pair registry under its own single key,
        /// so a second call for Default is rejected immediately.
        /// The setup action is NOT executed here — it is deferred until Build().
        /// </summary>
        public TypeAdapterConfigBuilder AddDefault(
            Action<DefaultOnlySetter> setup)
        {
            var name = "Default";

            return AddMappingStep(
                kind: MappingKind.DefaultSetter,
                name: name,
                sourceType: typeof(MapsterCacheMarker),
                destinationType: typeof(DefaultOnlySetter),
                twoWays: false,
                apply: cfg =>
                {
                    // Default is just another settings block —
                    // apply the same kind of setup as for pair rules.
                    var setter = cfg.Default;
                    setup(setter);
                });
        }

        /// <summary>
        /// Adds a NEW mapping rule for a type pair.
        /// Delegates to AddMappingStep with MappingKind.NewConfig.
        /// </summary>
        public TypeAdapterConfigBuilder NewConfig<TSource, TDestination>(
            Action<TypeAdapterSetter<TSource, TDestination>> setup)
        {
            var name = $"NewConfig<{typeof(TSource).Name}, {typeof(TDestination).Name}>";

            return AddMappingStep(
                kind: MappingKind.NewConfig,
                name: name,
                sourceType: typeof(TSource),
                destinationType: typeof(TDestination),
                twoWays: false,
                apply: cfg =>
                {
                    var setter = cfg.NewConfig<TSource, TDestination>();
                    setup(setter);
                });
        }

        /// <summary>
        /// Same as AddMapping but for types known only at runtime.
        /// Uses the same NewConfig key, so generic and non-generic
        /// registrations for one pair still collide — as intended.
        /// </summary>
        public TypeAdapterConfigBuilder NewConfig(
            Type sourceType,
            Type destinationType,
            Action<TypeAdapterSetter> setup)
        {
            var name = $"NewConfig({sourceType.Name} -> {destinationType.Name})";

            return AddMappingStep(
                kind: MappingKind.NewConfig,
                name: name,
                sourceType: sourceType,
                destinationType: destinationType,
                twoWays: false,
                apply: cfg =>
                {
                    var setter = cfg.NewConfig(sourceType, destinationType);
                    setup(setter);
                });
        }


        /// <summary>
        /// Adds a DESTINATION-ONLY rule: applies to all types assignable
        /// to TDestination. Keyed in its own namespace ("Destination:..."),
        /// so it never collides with pair-based mappings.
        /// </summary>
        public TypeAdapterConfigBuilder ForDestinationType<TDestination>(
            Action<TypeAdapterSetter<TDestination>> setup)
        {
            var name = $"ForDestinationType<{typeof(TDestination).Name}>";

            return AddMappingStep(
                kind: MappingKind.DestinationType,
                name: name,
                sourceType: null,
                destinationType: typeof(TDestination),
                twoWays: false,
                apply: cfg =>
                {
                    var setter = cfg.ForDestinationType<TDestination>();
                    setup(setter);
                });
        }


        /// <summary>
        /// Adds a DESTIONATION-ONLY rule when the destination type
        /// is known only at runtime (non-generic overload of ForDestinationType).
        /// Applies to all types assignable to destinationType.
        /// Keyed in its own namespace ("Destination:..."), so it never collides
        /// with pair-based mappings, but DOES collide with the generic version
        /// for the same destination type — as intended.
        /// The setup action is NOT executed here — it is deferred until Build().
        /// </summary>
        public TypeAdapterConfigBuilder ForDestinationType(
            Type destinationType,
            Action<DestinationTypeOnlySetter> setup)
        {
            var name = $"ForDestinationType({destinationType.Name})";

            return AddMappingStep(
                kind: MappingKind.DestinationType,
                name: name,
                sourceType: null,          // no source type for destination-only rules
                destinationType: destinationType,
                twoWays: false,
                apply: cfg =>
                {
                    // Non-generic overload: destination type resolved at runtime.
                    // Rules are set by member names, not by lambdas.
                    var setter = cfg.ForDestinationType(destinationType);
                    setup(setter);
                });
        }

        /// <summary>
        /// Adds a CONDITIONAL rule based on (source, dest, MapType).
        /// Conditional rules can coexist — no pair-key guard is needed.
        /// </summary>
        public TypeAdapterConfigBuilder When(
            string description,
            Func<Type, Type, MapType, bool> canMap,
            Action<TypeAdapterSetter> setup)
        {
            var name = $"When({description})";

            _steps.Add((name, cfg =>
            {
                var setter = cfg.When(canMap);
                setup(setter);
            }
            ));

            return this;
        }


        /// <summary>
        /// Adds a CONDITIONAL rule based on PreCompileArgument.
        /// Richer context than the (Type, Type, MapType) overload:
        /// member expressions and attributes are available inside the predicate.
        /// Conditional rules can coexist — no pair-key guard is needed.
        /// The setup action is NOT executed here — it is deferred until Build().
        /// </summary>
        public TypeAdapterConfigBuilder When(
            string description,
            Func<PreCompileArgument, bool> canMap,
            Action<TypeAdapterSetter> setup)
        {
            var name = $"When(PreCompileArgument: {description})";

            _steps.Add((name, cfg =>
            {
                // When returns a TypeAdapterSetter, so the user setup
                // can chain Map/Ignore/AfterMapping etc. on top of it.
                // The predicate is evaluated per member at rule-compile time,
                // not on every Adapt call.
                var setter = cfg.When(canMap);
                setup(setter);
            }
            ));

            return this;
        }

        /// <summary>
        /// Adds a TWO-WAYS mapping.
        /// TwoWays() returns a new root setter (TwoWaysTypeAdapterSetter)
        /// which holds two inner setters (forward and reverse) and applies
        /// every rule to BOTH directions. Mirrored methods (NameMatchingStrategy,
        /// IgnoreMember, GetMemberName) have their sides flipped automatically.
        /// Claims both directions in the pair registry: a later registration
        /// for the same pair in either direction is rejected.
        /// The setup action is NOT executed here — it is deferred until Build().
        /// </summary>
        public TypeAdapterConfigBuilder AddTwoWaysMapping<TSource, TDestination>(
            Action<TwoWaysTypeAdapterSetter<TSource, TDestination>> setup)
        {
            var name = $"NewConfig<{typeof(TSource).Name}, {typeof(TDestination).Name}> (TwoWays)";

            return AddMappingStep(
                kind: MappingKind.NewConfig,
                name: name,
                sourceType: typeof(TSource),
                destinationType: typeof(TDestination),
                twoWays: true, // claims both directions in the registry
                apply: cfg =>
                {
                    // TwoWays() creates a fresh TwoWaysTypeAdapterSetter rooted
                    // at this config; every rule applied inside setup goes
                    // to both directions.
                    var twoWaysSetter = cfg.NewConfig<TSource, TDestination>().TwoWays();
                    setup(twoWaysSetter);
                });
        }

        /// <summary>
        /// Adds a TWO-WAYS mapping with direction-specific forward rules.
        /// forwardSetup runs on the plain setter BEFORE TwoWays(),
        /// so those rules apply ONLY to the forward direction (Source -> Destination).
        /// twoWaysSetup runs on the TwoWaysTypeAdapterSetter AFTER TwoWays(),
        /// so those rules apply to BOTH directions.
        /// </summary>
        public TypeAdapterConfigBuilder AddTwoWaysMapping<TSource, TDestination>(
            Action<TypeAdapterSetter<TSource, TDestination>> forwardSetup,
            Action<TwoWaysTypeAdapterSetter<TSource, TDestination>> twoWaysSetup)
        {
            var name = $"NewConfig<{typeof(TSource).Name}, {typeof(TDestination).Name}> " +
                       $"(Forward + TwoWays)";

            return AddMappingStep(
                kind: MappingKind.NewConfig,
                name: name,
                sourceType: typeof(TSource),
                destinationType: typeof(TDestination),
                twoWays: true,
                apply: cfg =>
                {
                    // Order matters: forward-only rules must be set before
                    // TwoWays() is invoked, because TwoWays() returns a NEW
                    // wrapper setter and does not capture earlier rules.
                    var setter = cfg.NewConfig<TSource, TDestination>();
                    forwardSetup?.Invoke(setter);
                    twoWaysSetup?.Invoke(setter.TwoWays());
                });
        }



        #endregion TypeAdapterSetterSettings

        #region BoolSettings
        // ---- Boolean settings ----


        public TypeAdapterConfigBuilder ActivateCustomConvertersSrcNullPropagation(bool value) =>
        AddPropertyStep(
            propertyKey: nameof(TypeAdapterConfig.ActivateCustomConvertersSrcNullPropagation),
            displayValue: value.ToString(),
            value: value,
            apply: (cfg, v) => cfg.ActivateCustomConvertersSrcNullPropagation = v);

        /// <summary>Allows mapping to inherit members from implicit destination base types.</summary>
        public TypeAdapterConfigBuilder AllowImplicitDestinationInheritance(bool value) =>
        AddPropertyStep(
            propertyKey: nameof(TypeAdapterConfig.AllowImplicitDestinationInheritance),
            displayValue: value.ToString(),
            value: value,
            apply: (cfg, v) => cfg.AllowImplicitDestinationInheritance = v);

        /// <summary>Allows mapping to inherit members from implicit source base types.</summary>
        public TypeAdapterConfigBuilder AllowImplicitSourceInheritance(bool value) =>
        AddPropertyStep(
            propertyKey: nameof(TypeAdapterConfig.AllowImplicitSourceInheritance),
            displayValue: value.ToString(),
            value: value,
            apply: (cfg, v) => cfg.AllowImplicitSourceInheritance = v);

        /// <summary>Forces explicit mapping registration for every type pair.</summary>
        public TypeAdapterConfigBuilder RequireExplicitMapping(bool value) =>
        AddPropertyStep(
            propertyKey: nameof(TypeAdapterConfig.RequireExplicitMapping),
            displayValue: value.ToString(),
            value: value,
            apply: (cfg, v) => cfg.RequireExplicitMapping = v);


        /// <summary>Forces explicit mapping registration for primitive types as well.</summary>
        public TypeAdapterConfigBuilder RequireExplicitMappingPrimitive(bool value) =>
        AddPropertyStep(
            propertyKey: nameof(TypeAdapterConfig.RequireExplicitMappingPrimitive),
            displayValue: value.ToString(),
            value: value,
            apply: (cfg, v) => cfg.RequireExplicitMappingPrimitive = v);

        /// <summary>Requires every destination member to have a source or an explicit ignore.</summary>
        public TypeAdapterConfigBuilder RequireDestinationMemberSource(bool value) =>
        AddPropertyStep(
            propertyKey: nameof(TypeAdapterConfig.RequireDestinationMemberSource),
            displayValue: value.ToString(),
            value: value,
            apply: (cfg, v) => cfg.RequireDestinationMemberSource = v);

        /// <summary>
        /// Generates mapping code fully self-contained (no cached compiled projections),
        /// useful when code must survive config changes or be serialized.
        /// </summary>
        public TypeAdapterConfigBuilder SelfContainedCodeGeneration(bool value) =>
        AddPropertyStep(
            propertyKey: nameof(TypeAdapterConfig.SelfContainedCodeGeneration),
            displayValue: value.ToString(),
            value: value,
            apply: (cfg, v) => cfg.SelfContainedCodeGeneration = v);


        #endregion BoolSettings

        // ---- Special (non-boolean) settings ----

        /// <summary>
        /// Overrides the compiler used to turn lambda expressions into delegates.
        /// Name is descriptive only — a delegate value cannot be meaningfully printed.
        /// </summary>
        public TypeAdapterConfigBuilder Compiler(
            string description,
            Func<LambdaExpression, Delegate> compiler)
        {
            _steps.Add(($"Compiler({description})", cfg => cfg.Compiler = compiler));
            return this;
        }
       
    }
}
