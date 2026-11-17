using System;
using System.Collections.Generic;
using System.Text;

namespace Mapster.TypeAdapterConfigs.Builders
{
    /// <summary>
    /// Common base for all builder flavors. Holds the shared engine;
    /// the flavor itself is defined by which feature markers the
    /// concrete builder implements.
    /// </summary>
    public abstract class ConfigBuilderBase : IConfigBuilderStepsCore
    {
        private readonly ConfigBuilderEngine _engine = new();

        void IConfigBuilderStepsCore.RegisterPropertyStep<T>(
            string propertyKey, string displayValue, T value, Action<TypeAdapterConfig, T> apply)
            => _engine.AddPropertyStep(propertyKey, displayValue, value, apply);

        void IConfigBuilderStepsCore.RegisterMappingStep(
            MappingKind kind, string name, Type sourceType, Type destinationType, bool twoWays, Action<TypeAdapterConfig> apply)
            => _engine.AddMappingStep(kind, name, sourceType, destinationType, twoWays, apply);

        void IConfigBuilderStepsCore.RegisterStep(string name, Action<TypeAdapterConfig> apply)
            => _engine.AddStep(name, apply);

        public IReadOnlyList<string> GetSteps() => _engine.GetSteps();

        public (TypeAdapterConfig Config, IReadOnlyList<string> Steps) Build() => _engine.Build();
    }
}
