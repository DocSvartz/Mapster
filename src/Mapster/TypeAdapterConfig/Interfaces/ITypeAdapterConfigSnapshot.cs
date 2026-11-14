using Mapster.Cache;
using Mapster.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Mapster
{
    public interface ITypeAdapterConfigSnapshot : IGetRulesReadOnly
    {
        internal ConcurrentDictionary<Type, MappingTypeModel> TypeMemberModelsCache { get; init; }
        internal ConcurrentDictionary<TypeTuple, CompiledSettingsCacheItem> CacheCompiledTypesSettings { get; init; }
        internal IReadOnlyDictionary<string, bool> BoolSettings { get; init; }
        internal Delegate GetMapFunction(Type sourceType, Type destinationType);
        internal Delegate GetMapToTargetFunction(Type sourceType, Type destinationType);
        internal MethodCallExpression GetProjectionCallExpression(Type sourceType, Type destinationType);
        internal Expression CreateMapInvokeExpressionBody(Type sourceType, Type destinationType, Expression p);
        internal Expression CreateDynamicMapInvokeExpressionBody(Type destinationType, Expression p1);
        internal Expression CreateMapToTargetInvokeExpressionBody(Type sourceType, Type destinationType, Expression p1, Expression p2);
        internal LambdaExpression CreateInlineMapExpression(Type sourceType, Type destinationType, MapType mapType, CompileContext context, MemberMapping? mapping = null);

        ITypeAdapterConfig Config { get; init; }
        IReadOnlyCollection<TypeAdapterRule> Rules { get; }
        
        ulong Version { get; init; }

        void Compile(bool failFast = true);
        void Compile(Type sourceType, Type destinationType);
        void CompileProjection();
        void CompileProjection(Type sourceType, Type destinationType);
        LambdaExpression CreateMapExpression(TypeTuple tuple, MapType mapType);
        bool GetBoolSetting(string name);
        Func<object, TDestination> GetDynamicMapFunction<TDestination>(Type sourceType);
        Func<TSource, TDestination> GetMapFunction<TSource, TDestination>();
        Func<TSource, TDestination, TDestination> GetMapToTargetFunction<TSource, TDestination>();

    }
}
