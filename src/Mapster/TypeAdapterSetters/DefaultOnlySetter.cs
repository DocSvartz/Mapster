namespace Mapster.TypeAdapterSetters
{
    [AdaptWith(AdaptDirectives.DestinationAsRecord)]
    public class DefaultOnlySetter
    {
        protected const string SourceParameterName = "source";
        protected const string ResultParameterName = "result";
        protected const string DestinationParameterName = "destination";

        public readonly IDefaultTypeAdapterSettings Settings;
        public readonly TypeAdapterConfig Config;
        public DefaultOnlySetter(TypeAdapterSettings settings, TypeAdapterConfig config)
        {
            Settings = settings;
            Config = config;
        }
    }
}
