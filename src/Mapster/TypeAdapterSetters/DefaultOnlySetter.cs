namespace Mapster.TypeAdapterSetters
{
    [AdaptWith(AdaptDirectives.DestinationAsRecord)]
    public class DefaultOnlySetter: BaseSetter
    {
        protected const string SourceParameterName = "source";
        protected const string ResultParameterName = "result";
        protected const string DestinationParameterName = "destination";

        public IDefaultTypeAdapterSettings Settings => OriginSettings;
        public DefaultOnlySetter(TypeAdapterSettings originSettings, ITypeAdapterConfigRuleProducer config):base(originSettings, config)
        {
        }
    }


    public abstract class BaseSetter
    {
        public readonly TypeAdapterSettings OriginSettings;
        public readonly ITypeAdapterConfigRuleProducer Config;
        public BaseSetter(TypeAdapterSettings originSettings, ITypeAdapterConfigRuleProducer config)
        {
            OriginSettings = originSettings;
            Config = config;
        }
    }


}
