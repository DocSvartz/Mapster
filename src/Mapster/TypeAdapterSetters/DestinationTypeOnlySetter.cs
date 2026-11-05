namespace Mapster.TypeAdapterSetters
{
    [AdaptWith(AdaptDirectives.DestinationAsRecord)]
    public class DestinationTypeOnlySetter : DefaultOnlySetter
    {
        public new readonly IDestinationTypeAdapterSettings Settings;
        public DestinationTypeOnlySetter(TypeAdapterSettings settings, TypeAdapterConfig config): base(settings, config)
        {
            Settings = settings;
        }
    }
}
