using System;
using System.Collections.Generic;

namespace Mapster.TypeAdapterConfigs.Builders
{
    /// <summary>
    /// Registration plumbing shared by all builder flavors.
    /// Implemented explicitly, so it never shows up in IntelliSense
    /// on the concrete builders.
    /// </summary>
    public interface IConfigBuilderStepsCore
    {
        void RegisterPropertyStep<T>(string propertyKey, string displayValue, T value, Action<TypeAdapterConfig, T> apply);
        void RegisterMappingStep(MappingKind kind, string name, Type sourceType, Type destinationType, bool twoWays, Action<TypeAdapterConfig> apply);
        void RegisterStep(string name, Action<TypeAdapterConfig> apply);
        IReadOnlyList<string> GetSteps();
        (TypeAdapterConfig Config, IReadOnlyList<string> Steps) Build();
    }
}
