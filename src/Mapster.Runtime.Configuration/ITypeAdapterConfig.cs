using Mapster.Configuration;
using System;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Mapster.Runtime.Configuration
{
    public interface ITypeAdapterConfig : ITypeAdapterConfigBase
    {
       // static ITypeAdapterConfig GlobalSettings { get; }
       // static System.Collections.Generic.List<TypeAdapterRule> RulesTemplate { get; }
        bool AllowImplicitDestinationInheritance { get; set; }
        bool AllowImplicitSourceInheritance { get; set; }
        Func<LambdaExpression, Delegate> Compiler { get; set; }
        TypeAdapterSetter Default { get; }
        bool RequireDestinationMemberSource { get; set; }
        bool RequireExplicitMapping { get; set; }
        bool RequireExplicitMappingPrimitive { get; set; }
       // System.Collections.Concurrent.ConcurrentDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; }
      //  System.Collections.Generic.List<TypeAdapterRule> Rules { get; }
        bool SelfContainedCodeGeneration { get; set; }

       // void Apply(System.Collections.Generic.IEnumerable<IRegister> registers);
       // void Apply(System.Collections.Generic.IEnumerable<Lazy<IRegister>> registers);
       // void Apply(params IRegister[] registers);
        ITypeAdapterConfig Clone();
        void Compile(bool failFast = true);
        void Compile(Type sourceType, Type destinationType);
        void CompileProjection();
        void CompileProjection(Type sourceType, Type destinationType);
       // LambdaExpression CreateMapExpression(TypeTuple tuple, MapType mapType);
        TypeAdapterSetter ForDestinationType(Type destinationType);
        TypeAdapterSetter<TDestination> ForDestinationType<TDestination>();
        ITypeAdapterConfig Fork(Action<ITypeAdapterConfig> action, [CallerFilePath] string key1 = "", [CallerLineNumber] int key2 = 0);
        TypeAdapterSetter ForType(Type sourceType, Type destinationType);
        TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>();
        Func<object, TDestination> GetDynamicMapFunction<TDestination>(Type sourceType);
        Func<TSource, TDestination> GetMapFunction<TSource, TDestination>();
        Func<TSource, TDestination, TDestination> GetMapToTargetFunction<TSource, TDestination>();
        TypeAdapterSetter NewConfig(Type sourceType, Type destinationType);
        TypeAdapterSetter<TSource, TDestination> NewConfig<TSource, TDestination>();
       // System.Collections.Generic.IList<IRegister> Scan(params Assembly[] assemblies);
       // TypeAdapterSetter When(Func<PreCompileArgument, bool> canMap);
       // TypeAdapterSetter When(Func<Type, Type, MapType, bool> canMap);
    }
}