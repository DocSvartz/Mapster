using Mapster.Adapters;
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
        void RegisterPropertyStep<T>(string propertyKey, string displayValue, T value, Action<BuilderTypeAdapterConfig, T> apply);
        void RegisterMappingStep(MappingKind kind, string name, Type sourceType, Type destinationType, bool twoWays, Action<BuilderTypeAdapterConfig> apply);
        void RegisterAdapterStep<TAdapter>(string name, Func<TAdapter> adapterFactory, Action<BuilderTypeAdapterConfig, TAdapter> apply) where TAdapter : BaseAdapter;
        void RegisterStep(string name, Action<BuilderTypeAdapterConfig> apply);
        IReadOnlyList<string> GetSteps();
        (BuilderTypeAdapterConfig Config, IReadOnlyList<string> Steps) Build();
    }
}
