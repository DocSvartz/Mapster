using Mapster.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

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
        List<TypeAdapterRule> Rules { get; }
        bool SelfContainedCodeGeneration { get; set; }

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
        void Remove(Type sourceType, Type destinationType);
    }
}