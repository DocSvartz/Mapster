using Mapster;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mapster
{
    /// <summary>
    /// Lazy builder for Mapster's TypeAdapterConfig.
    /// Collects configuration steps without applying them.
    /// Guards:
    ///   - each config property can be set only once;
    ///   - each type pair can have only one mapping rule
    ///     (NewConfig and ForType for the same pair conflict with each other).
    /// </summary>
    public sealed partial class TypeAdapterConfigBuilder
    {
        // Kind of a mapping step: needed to distinguish direction
        // and to control whether ForType is allowed on top of NewConfig.
        private enum MappingKind
        {
            NewConfig,          // replaces rules for the pair
            ForType,            // enhances rules for the pair
            DestinationType,    // destination-only rule (keyed by destination type)
            DefaultSetter,            // fallback settings for unmapped pairs (config-level)
        }

        private readonly List<(string Name, Action<TypeAdapterConfig> Apply)> _steps = new();

        // Properties already set — each may appear at most once.
        private readonly HashSet<string> _appliedProperties = new(StringComparer.Ordinal);

        // Type pairs already registered — each pair may appear at most once.
        // For TwoWays mappings the pair is registered in both directions,
        // so that a reverse NewConfig cannot silently overwrite the original.
        private readonly HashSet<string> _appliedMappings = new(StringComparer.Ordinal);

        private TypeAdapterConfigBuilder() { }

        public static TypeAdapterConfigBuilder Create() => new();

        // ------------------------------------------------------------------
        // Registration core: two entry points with duplicate guards.
        // ------------------------------------------------------------------

        /// <summary>
        /// Registers a CONFIG PROPERTY step (boolean/int/compiler etc.).
        /// Throws if the same property is already configured.
        /// </summary>
        private TypeAdapterConfigBuilder AddPropertyStep<T>(
            string propertyKey,
            string displayValue,
            T value,
            Action<TypeAdapterConfig, T> apply)
        {
            if (!_appliedProperties.Add(propertyKey))
            {
                throw new InvalidOperationException(
                    $"Setting '{propertyKey}' is already configured in this builder. " +
                    "Each property can be set only once to keep the configuration deterministic.");
            }

            var name = $"{propertyKey} = {displayValue}";
            _steps.Add((name, cfg => apply(cfg, value)));
            return this;
        }

        /// <summary>
        /// Registers a MAPPING step keyed by (source, destination) types.
        /// Throws if the same pair is already registered in any direction.
        /// </summary>
        private TypeAdapterConfigBuilder AddMappingStep(
            MappingKind kind,
            string name,
            Type sourceType,
            Type destinationType,
            bool twoWays,
            Action<TypeAdapterConfig> apply)
        {
            var keys = BuildPairKeys(kind, sourceType, destinationType, twoWays);

            // Any collision means the pair is already described by another step:
            // either exact duplicate, or the reverse direction of a TwoWays rule,
            // or ForType on top of NewConfig for the same pair.
            foreach (var key in keys)
            {
                if (!_appliedMappings.Add(key))
                {
                    throw new InvalidOperationException(
                        $"Mapping for the pair is already registered in this builder: " +
                        $"'{DescribePair(sourceType, destinationType, kind, twoWays)}'. " +
                        "Each type pair can be registered only once to keep the configuration unambiguous.");
                }
            }

            _steps.Add((name, apply));
            return this;
        }

        /// <summary>
        /// Builds uniqueness keys for a mapping step.
        /// Directional pair always produces its own key;
        /// a TwoWays mapping also claims the reverse direction,
        /// so it blocks both A->B and B->A registrations afterwards.
        /// </summary>
        private static string[] BuildPairKeys(
            MappingKind kind,
            Type sourceType,
            Type destinationType,
            bool twoWays)
        {
            // Destination-only rules live in their own namespace of keys,
            // so they never collide with pair-based mappings.
            if (kind == MappingKind.DestinationType)
            {
                return new[] { $"Destination:{destinationType.FullName}" };
            }

            var forward = $"{sourceType.FullName}|{destinationType.FullName}";
            return twoWays
                ? new[] { forward, $"{destinationType.FullName}|{sourceType.FullName}" }
                : new[] { forward };
        }

        private static string DescribePair(
            Type sourceType,
            Type destinationType,
            MappingKind kind,
            bool twoWays)
        {
            var src = sourceType is null ? "<any>" : sourceType.Name;
            var dst = destinationType.Name;
            var arrow = twoWays ? " <-> " : " -> ";
            return $"{kind}({src}{arrow}{dst})";
        }

        // ------------------------------------------------------------------
        // Shape of the public step methods (not implemented here).
        // Each of them delegates to one of the two entry points:
        //
        //   - property steps  -> AddPropertyStep(key, displayValue, value, apply)
        //   - mapping steps   -> AddMappingStep(kind, name, src, dest, twoWays, apply)
        //
        // Examples of keys produced:
        //   NewConfig<User, UserDto>            -> "Ns.User|Ns.UserDto"
        //   NewConfig with TwoWays              -> "Ns.User|Ns.UserDto" + "Ns.UserDto|Ns.User"
        //   ForType<User, UserDto>              -> same key as NewConfig — collision, rejected
        //   ForDestinationType<UserDto>         -> "Destination:Ns.UserDto"
        // ------------------------------------------------------------------

        // ---- History & build ----

        public IReadOnlyList<string> GetSteps() =>
            _steps.Select(s => s.Name).ToList().AsReadOnly();

        public (TypeAdapterConfig Config, IReadOnlyList<string> Steps) Build()
        {
            var config = new TypeAdapterConfig();
            var history = new List<string>();

            foreach (var (name, apply) in _steps)
            {
                history.Add(name);
                apply(config);
            }

            history.Add("Build completed");
            return (config, history.AsReadOnly());
        }
    }

}
