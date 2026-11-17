namespace Mapster.TypeAdapterSetters
{
    [AdaptWith(AdaptDirectives.DestinationAsRecord)]
    public class DestinationTypeOnlySetter : DefaultOnlySetter
    {
        public new IDestinationTypeAdapterSettings Settings => OriginSettings;
        public DestinationTypeOnlySetter(TypeAdapterSettings originSettings, ITypeAdapterConfigRuleProducer config): base(originSettings, config)
        {
        }
    }
}
