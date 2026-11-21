using Mapster.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mapster
{
    public interface ITypeAdapterConfig :
        ITypeAdapterConfigAdapterConsumer,
        ITypeAdapterConfigCloneable,
        ITypeAdapterConfigForkable,
        ITypeAdapterConfigGetSnapshot,
        ITypeAdapterConfigSettingsProducer
    {
        
        IReadOnlyDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; }
        IList<TypeAdapterRule> Rules { get; }



         internal ITypeAdapterConfig Fork(Action<ITypeAdapterConfig> action, string key1, int key2);



        IList<IRegister> Scan(params Assembly[] assemblies);
        void Apply(IEnumerable<IRegister> registers);
        void Apply(IEnumerable<Lazy<IRegister>> registers);
        void Apply(params IRegister[] registers);
    }
}