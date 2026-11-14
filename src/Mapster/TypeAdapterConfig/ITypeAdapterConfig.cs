using Mapster.Cache;
using Mapster.Models;
using Mapster.TypeAdapterSetters;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mapster
{
    public interface ITypeAdapterConfig
    {
        bool ActivateCustomConvertersSrcNullPropagation { get; set; }
        bool AllowImplicitDestinationInheritance { get; set; }
        bool AllowImplicitSourceInheritance { get; set; }
        bool RequireDestinationMemberSource { get; set; }
        bool RequireExplicitMapping { get; set; }
        bool RequireExplicitMappingPrimitive { get; set; }
        bool SelfContainedCodeGeneration { get; set; }
        Func<LambdaExpression, Delegate> Compiler { get; set; }
        
        
       
        ConcurrentDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; }
        IList<TypeAdapterRule> Rules { get; }
        public SettingsCacheStorage MappingCache { get; }

        DefaultOnlySetter Default { get; }
        DestinationTypeOnlySetter ForDestinationType(Type destinationType);
        TypeAdapterSetter<TDestination> ForDestinationType<TDestination>();
        TypeAdapterSetter ForType(Type sourceType, Type destinationType);
        TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>();
        TypeAdapterSetter NewConfig(Type sourceType, Type destinationType);
        TypeAdapterSetter<TSource, TDestination> NewConfig<TSource, TDestination>();
        TypeAdapterSetter When(Func<PreCompileArgument, bool> canMap);
        TypeAdapterSetter When(Func<Type, Type, MapType, bool> canMap);

        ITypeAdapterConfig Fork(Action<ITypeAdapterConfig> action, [CallerFilePath] string key1 = "", [CallerLineNumber] int key2 = 0);
        IList<IRegister> Scan(params Assembly[] assemblies);
        void Apply(IEnumerable<IRegister> registers);
        void Apply(IEnumerable<Lazy<IRegister>> registers);
        void Apply(params IRegister[] registers);
        ITypeAdapterConfig Clone();
        void Compile(bool failFast = true);
        void Compile(Type sourceType, Type destinationType);
        void CompileProjection();
        void CompileProjection(Type sourceType, Type destinationType);

        ITypeAdapterConfigSnapshot GetSnapshot { get; }
    }
}