using Mapster.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mapster
{
    public interface ITypeAdapterConfig :
        ITypeAdapterConfigRuleProducer,
        ITypeAdapterConfigRuleMapProducer,
        ITypeAdapterConfigCustomWhenRuleProducer,
        ITypeAdapterConfigAdapterConsumer,
        ITypeAdapterConfigCloneable,
        ITypeAdapterConfigForkable,
        ITypeAdapterConfigGetSnapshot
    {
        
        IReadOnlyDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; }
        IList<TypeAdapterRule> Rules { get; }
        //public SettingsCacheStorage MappingCache { get; }

       
        
        

        
        IList<IRegister> Scan(params Assembly[] assemblies);
        void Apply(IEnumerable<IRegister> registers);
        void Apply(IEnumerable<Lazy<IRegister>> registers);
        void Apply(params IRegister[] registers);
        
        void Compile(bool failFast = true);
        void CompileToSettingsSetters(Type sourceType, Type destinationType);
        void CompileProjection();
        void CompileProjectionToSettingsSetters(Type sourceType, Type destinationType);

    }
}