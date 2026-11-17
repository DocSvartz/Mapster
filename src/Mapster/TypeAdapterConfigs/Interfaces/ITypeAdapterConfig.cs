using Mapster.Cache;
using Mapster.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mapster
{
    public interface ITypeAdapterConfig : 
        ITypeAdapterConfigSettings,
        ITypeAdapterConfigRuleMapProducer,
        ITypeAdapterConfigCustomRuleProducer,
        ITypeAdapterConfigAdapterConsumer,
        ITypeAdapterConfigCloneable,
        ITypeAdapterConfigForkable,
        ITypeAdapterConfigGetSnapshot
    {
        
        IReadOnlyDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; }
        IList<TypeAdapterRule> Rules { get; }
        public SettingsCacheStorage MappingCache { get; }

       
        
        

        
        IList<IRegister> Scan(params Assembly[] assemblies);
        void Apply(IEnumerable<IRegister> registers);
        void Apply(IEnumerable<Lazy<IRegister>> registers);
        void Apply(params IRegister[] registers);
        
        void Compile(bool failFast = true);
        void Compile(Type sourceType, Type destinationType);
        void CompileProjection();
        void CompileProjection(Type sourceType, Type destinationType);

    }
}