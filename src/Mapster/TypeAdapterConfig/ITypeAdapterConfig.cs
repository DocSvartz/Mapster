using Mapster.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Mapster
{
    public interface ITypeAdapterConfig
    {
        bool AllowImplicitDestinationInheritance { get; set; }
        bool AllowImplicitSourceInheritance { get; set; }
        Func<LambdaExpression, Delegate> Compiler { get; set; }
        ConfigCompileStorage ConfigCompile { get; }
        bool IsGlobalSettings { get; }
        bool RequireDestinationMemberSource { get; set; }
        bool RequireExplicitMapping { get; set; }
        bool RequireExplicitMappingPrimitive { get; set; }
        ConcurrentDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; }
        //List<TypeAdapterRule> Rules { get; }
        void AddRule(TypeAdapterRule rule);
        public TypeAdapterSettings GetMergedSettings(TypeTuple tuple, MapType mapType);
        public IEnumerable<TypeAdapterRule> GetRules(Func<TypeAdapterRule, bool> predicate);
        bool SelfContainedCodeGeneration { get; set; }

        bool ConcurrencyEnvironment { get; }
        AutoResetEvent Configure {  get;}
        AutoResetEvent ApplySync { get; }

        void Apply(IEnumerable<IRegister> registers);
        void Clear();
        ITypeAdapterConfig Clone();
        void Compile(bool failFast = true);
        void Compile(Type sourceType, Type destinationType);
        void CompileProjection();
        void CompileProjection(Type sourceType, Type destinationType);
        LambdaExpression CreateMapExpression(TypeTuple tuple, MapType mapType);
        ITypeAdapterConfig Fork(Action<ITypeAdapterConfig> action, [CallerFilePath] string key1 = "", [CallerLineNumber] int key2 = 0);
        TypeAdapterSetter ForType(Type sourceType, Type destinationType);
        TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>();
        void Remove(Type sourceType, Type destinationType);
    }
}