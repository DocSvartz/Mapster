using Mapster.Adapters;
using System.Collections.Generic;

namespace Mapster.TypeAdapterConfigs.Builders.Interfaces
{
    internal interface ITypeAdapterConfigSettingsBuildOnlyCreator:
        ITypeAdapterConfigSettings,
        ITypeAdapterConfigRuleMapBuildOnlyProducer,
        ITypeAdapterConfigCustomWhenRuleProducer
    {
        public void AddRule(TypeAdapterRule rule);
        public void AddRulesRange(IEnumerable<TypeAdapterRule> rules);
        public void AddAdapter(BaseAdapter adapter);
        public void AddAdapterssRange(IEnumerable<BaseAdapter> adapters);
    }
}
