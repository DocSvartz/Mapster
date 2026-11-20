using Mapster.Cache;
using Mapster.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Mapster
{
    public abstract class BaseConfigSnapshot : BaseMapFuncProducer, ITypeAdapterConfigSnapshot
    {
        private bool? _RequireExplicitMapping;
        private bool? _RequireExplicitMappingPrimitive;
        private bool RequireExplicitMapping { get => _RequireExplicitMapping ?? GetBoolSetting(nameof(ITypeAdapterConfigSettings.RequireExplicitMapping)); }
        private bool RequireExplicitMappingPrimitive { get => _RequireExplicitMappingPrimitive ?? GetBoolSetting(nameof(ITypeAdapterConfigSettings.RequireExplicitMappingPrimitive)); }
        internal ITypeAdapterConfig Config { get; init; }
        internal ImmutableDictionary<string, bool> BoolSettings { get; init; }
        public bool GetBoolSetting(string name) { BoolSettings.TryGetValue(name, out var data); return data; }

        public ulong Version { get; init; }

        public TypeAdapterRule DefaultSettingsRule { get; init; }
        public IReadOnlyCollection<TypeAdapterRule> Rules => this;

        Func<LambdaExpression, Delegate> ITypeAdapterMappingFuncProducer.Compiler { get => Compiler; init => Compiler = value; }
        public ImmutableArray<TypeAdapterRule> TemplateRules { get; init; }
        public IReadOnlyCollection<TypeAdapterRule> AdapterRules { get; init; }
        public IReadOnlyCollection<TypeAdapterRule> CustomSettingsRules { get; init; }
        public IReadOnlyDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; init; }

        internal ConcurrentDictionary<Type, MappingTypeModel> TypeMemberModelsCache { get; init; } = new();
        internal ConcurrentDictionary<TypeTuple, CompiledSettingsCacheItem> CacheCompiledTypesSettings { get; init; } = new(); 

        public override LambdaExpression CreateMapExpression(TypeTuple tuple, MapType mapType)
        {
            var context = new CompileContext(this);
            context.Running.Add(tuple);
            Action<ITypeAdapterConfig>? fork = null;
            try
            {
                var arg = GetCompileArgument(tuple, mapType, context);
                fork = arg.Settings.Fork;
                if (fork != null)
                {
                    var cloned = Config.Clone();
                    fork(cloned);
                    context.Configs.Push(cloned.GetSnapshot);
                    arg.Settings = context.GetMergedSettings(tuple, mapType); // rebuild
                }

                // this.MappingCache.CollectCompileRules(context.CompiledRules);

                return CreateMapExpression(arg);
            }
            finally
            {
                // context.AttributeMetadata.Complete();
                if (fork != null)
                    context.Configs.Pop();
                context.Running.Remove(tuple);
            }
        }

        

        

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<TypeAdapterRule> GetEnumerator()
        {
            foreach (var x in TemplateRules) yield return x;
            foreach (var x in AdapterRules) yield return x;
            if (DefaultSettingsRule is not null) yield return DefaultSettingsRule;
            foreach (var x in CustomSettingsRules) yield return x;
            foreach (var x in RuleMap.Values) yield return x;
        }

        public int Count => DefaultSettingsRule is not null ? 1 : 0
                            + TemplateRules.Length
                            + AdapterRules.Count
                            + CustomSettingsRules.Count
                            + RuleMap.Count;

        ITypeAdapterConfig ITypeAdapterConfigSnapshot.Config { get => Config; init => Config = value; }
        ConcurrentDictionary<Type, MappingTypeModel> ITypeAdapterConfigSnapshot.TypeMemberModelsCache { get => TypeMemberModelsCache; init => TypeMemberModelsCache = value; }
        ConcurrentDictionary<TypeTuple, CompiledSettingsCacheItem> ITypeAdapterConfigSnapshot.CacheCompiledTypesSettings { get => CacheCompiledTypesSettings; init => CacheCompiledTypesSettings = value; }
        ImmutableDictionary<string, bool> ITypeAdapterConfigSnapshot.BoolSettings { get => BoolSettings; init => BoolSettings = value; }

        #region MappingFuncProducerImplimentation
        Expression ITypeAdapterMappingFuncProducer.CreateMapInvokeExpressionBody(Type sourceType, Type destinationType, Expression p)
        {
            var key = new TypeTuple(sourceType, destinationType);
            if (RequireExplicitMapping || RequireExplicitMappingPrimitive)
                Compiler(CreateMapExpression(key, MapType.Map));

            return CreateMapInvokeExpressionBody(key, p);
        }

        Expression ITypeAdapterMappingFuncProducer.CreateDynamicMapInvokeExpressionBody(Type destinationType, Expression p1)
        {
            return CreateDynamicMapInvokeExpressionBody(destinationType, p1);
        }

        Expression ITypeAdapterMappingFuncProducer.CreateMapToTargetInvokeExpressionBody(Type sourceType, Type destinationType, Expression p1, Expression p2)
        {
            if (RequireExplicitMapping || RequireExplicitMappingPrimitive)
            {
                var key = new TypeTuple(sourceType, destinationType);
                AddOrGetMapToTargetFuncFromCache(key, true);
            }

            return CreateMapToTargetInvokeExpressionBody(sourceType, destinationType, p1, p2);
        }

        LambdaExpression ITypeAdapterMappingFuncProducer.CreateInlineMapExpression(Type sourceType, Type destinationType, MapType mapType, CompileContext context, MemberMapping? mapping)
        {
            return CreateInlineMapExpression(sourceType, destinationType, mapType, context, mapping);
        }

        Delegate ITypeAdapterMappingFuncProducer.GetMapToTargetFunction(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            return AddOrGetMapToTargetFuncFromCache(key);
        }

        Delegate ITypeAdapterMappingFuncProducer.GetMapFunction(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            return AddOrGetMapFuncFromCache(key);
        }

        MethodCallExpression ITypeAdapterMappingFuncProducer.GetProjectionCallExpression(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            return AddOrGetProjectionFuncFromCache(key);
        }

        MethodCallExpression ITypeAdapterMappingFuncProducer.CreateProjectionCallExpression(TypeTuple tuple)
        {
            return CreateProjectionCallExpression(tuple);
        }

        public Func<TSource, TDestination, TDestination> GetMapToTargetFunction<TSource, TDestination>()
        {
            var key = TypeTuple.Create<TSource, TDestination>();
            return (Func<TSource, TDestination, TDestination>)AddOrGetMapToTargetFuncFromCache(key);
        }

        public Func<TSource, TDestination> GetMapFunction<TSource, TDestination>()
        {
            var key = TypeTuple.Create<TSource, TDestination>();
            return (Func<TSource, TDestination>)AddOrGetMapFuncFromCache(key);
        }

        public Expression<Func<TSource, TDestination>> GetProjectionExpression<TSource, TDestination>()
        {
            var del = AddOrGetProjectionFuncFromCache(TypeTuple.Create<TSource, TDestination>());

            return (Expression<Func<TSource, TDestination>>)((UnaryExpression)del.Arguments[1]).Operand;
        }

        public Func<object, TDestination> GetDynamicMapFunction<TDestination>(Type sourceType)
        {
            var key = new TypeTuple(sourceType, typeof(TDestination));
            return AddOrGetGetDynamicMapFunction<TDestination>(sourceType);
        }

        #endregion MappingFuncProducerImplimentation

    }
}
