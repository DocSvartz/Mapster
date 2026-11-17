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
        TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>();
        void Compile(Type sourceType, Type destinationType);
        void CompileProjection(Type sourceType, Type destinationType);
    }
}
