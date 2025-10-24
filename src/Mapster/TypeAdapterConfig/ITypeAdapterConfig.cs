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
        Func<object, TDestination> GetDynamicMapFunction<TDestination>(Type sourceType);
        Func<TSource, TDestination> GetMapFunction<TSource, TDestination>();
        Func<TSource, TDestination, TDestination> GetMapToTargetFunction<TSource, TDestination>();
        TypeAdapterSettings GetSettings(TypeTuple key);
        void Remove(Type sourceType, Type destinationType);
    }

    internal interface ITypeAdapterConfigInternal : ITypeAdapterConfig
    {
        internal Delegate GetMapFunction(Type sourceType, Type destinationType);
        internal Delegate GetMapToTargetFunction(Type sourceType, Type destinationType);
        internal Expression<Func<TSource, TDestination>> GetProjectionExpression<TSource, TDestination>();
        internal MethodCallExpression GetProjectionCallExpression(Type sourceType, Type destinationType);
        internal Expression CreateDynamicMapInvokeExpressionBody(Type destinationType, Expression p1);
        internal LambdaExpression CreateInlineMapExpression(Type sourceType, Type destinationType, MapType mapType, CompileContext context, MemberMapping? mapping = null);
        internal LambdaExpression CreateMapInvokeExpression(Type sourceType, Type destinationType, MapType mapType);
        internal Expression CreateMapInvokeExpressionBody(Type sourceType, Type destinationType, Expression p);
        internal Expression CreateMapToTargetInvokeExpressionBody(Type sourceType, Type destinationType, Expression p1, Expression p2);
        internal TypeAdapterSettings GetMergedSettings(TypeTuple tuple, MapType mapType);

    }
}