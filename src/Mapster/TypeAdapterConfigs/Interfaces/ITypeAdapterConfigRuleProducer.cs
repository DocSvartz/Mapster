using Mapster.Adapters;
using System;
using System.Collections.Generic;

namespace Mapster
{
    public interface ITypeAdapterConfigRuleProducer : ITypeAdapterConfigSettings
    {
        public void AddRule(TypeAdapterRule rule);
        public void AddRulesRange(IEnumerable<TypeAdapterRule> rules);
        public void AddAdapter(BaseAdapter adapter);
        public void AddAdapterssRange(IEnumerable<BaseAdapter> adapters);
        TypeAdapterSetter<TSource, TDestination> TwoWaysForType<TSource, TDestination>();
        void CompileToSettingsSetters(Type sourceType, Type destinationType);
        void CompileProjectionToSettingsSetters(Type sourceType, Type destinationType);
    }
}
