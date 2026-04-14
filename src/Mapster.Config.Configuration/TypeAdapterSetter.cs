using Mapster.Settings;

namespace Mapster.Configuration
{
    public abstract class TypeAdapterSetterBase<TAdapterSettings, TConfig> 
        where TAdapterSettings : TypeAdapterSettingsBase
        where TConfig : ITypeAdapterConfigBase

    {
        protected const string SourceParameterName = "source";
        protected const string ResultParameterName = "result";
        protected const string DestinationParameterName = "destination";

        public readonly TAdapterSettings Settings;
        public readonly TConfig Config;
        public TypeAdapterSetterBase(TAdapterSettings settings, TConfig config)
        {
            Settings = settings;
            Config = config;
        }
    }

    public class TypeAdapterSetter : TypeAdapterSetterBase<TypeAdapterSettingsBase, ITypeAdapterConfigBase>
    {
        public TypeAdapterSetter(TypeAdapterSettingsBase settings, ITypeAdapterConfigBase config) : base(settings, config)
        {
        }
    }

    public class TypeAdapterSetter<TDestination> : TypeAdapterSetterBase<TypeAdapterSettingsBase, ITypeAdapterConfigBase>
    {
        public TypeAdapterSetter(TypeAdapterSettingsBase settings, ITypeAdapterConfigBase config) : base(settings, config)
        {
        }
    }

    public class TypeAdapterSetter<TSource,TDestination> : TypeAdapterSetter<TDestination>
    {
        public TypeAdapterSetter(TypeAdapterSettingsBase settings, ITypeAdapterConfigBase config) : base(settings, config)
        {
        }
    }

    public class TwoWaysTypeAdapterSetter<TSource, TDestination> : TypeAdapterSetter<TSource, TDestination>
    {
        public TwoWaysTypeAdapterSetter(TypeAdapterSettingsBase settings, ITypeAdapterConfigBase config) : base(settings, config)
        {
        }
    }
}
