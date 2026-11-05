namespace Mapster.TypeAdapterSetters
{
    [AdaptWith(AdaptDirectives.DestinationAsRecord)]
    public class DestinationTypeOnlySetter : DefaultOnlySetter
    {
        public new IDestinationTypeAdapterSettings Settings => OriginSettings;
        public DestinationTypeOnlySetter(TypeAdapterSettings originSettings, TypeAdapterConfig config): base(originSettings, config)
        {
        }
    }
}
