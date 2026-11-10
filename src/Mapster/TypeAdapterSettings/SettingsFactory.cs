using Mapster.Adapters;

namespace Mapster
{
    public static class SettingsFactory
    {
        /// <summary>
        /// Settings used for creating mapping rule <see cref="CustomAdapterRule"/> in: 
        /// <see cref="BaseAdapter"/>,
        /// <see cref="CustomAdapterRule"/>
        /// .
        /// </summary>
        public static TypeAdapterSettings GetFinalizeSettings => new TypeAdapterSettings() { IsFinalized = true };

        /// <summary>
        /// Settings used for creating mapping rule <see cref="ExplicitTypeAdapterRule"/> in: 
        /// <see cref="TypeAdapterConfig.NewConfig"/>, 
        /// <see cref="TypeAdapterConfig.ForType"/>, 
        /// <see cref="TypeAdapterConfig.ForDestinationType"/>
        /// .
        /// DefaultSetter
        /// </summary>
        internal static TypeAdapterSettings GetExplicitTypeSettings => new TypeAdapterSettings() { IsFinalized = false };
    }
}
