using Mapster.Cache;
using Mapster.Models;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

namespace Mapster
{
    public abstract class BaseConfigSnapshot : BaseMapFuncProducer, ITypeAdapterConfigSnapshot
    {    
        
        public abstract IReadOnlyCollection<TypeAdapterRule> DefaultSettingsRules { get; init; }
        public abstract IReadOnlyCollection<TypeAdapterRule> AdapterRules { get; init; }
        public abstract IReadOnlyCollection<TypeAdapterRule> CustomSettingsRules { get; init; }
        public abstract IReadOnlyDictionary<TypeTuple, TypeAdapterRule> RuleMap { get; init; }

        public ulong Version { get; init; }

        public TypeAdapterRule DefaultSettingsRule { get; init; }
        public IReadOnlyCollection<TypeAdapterRule> Rules => this;
        internal ITypeAdapterConfig Config { get; init; }

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

        /// <summary>
        /// Validates and cache mapping instructions.
        /// </summary>
        /// <param name="failFast">A boolean parameter that determines whether exceptions should be thrown immediately when mapping errors occur or whether to collect and aggregate them. The default value is true.</param>
        /// <exception cref="AggregateException"></exception>
        public void Compile(bool failFast = true)
        {
            var exceptions = new List<Exception>();
            var keys = RuleMap.Keys.ToList();

            foreach (var key in keys)
            {
                try
                {
                    if (key.Source == typeof(void))
                        continue;

                    if (key.Source.ContainsGenericParameters || key.Destination.ContainsGenericParameters)
                        continue;

                    _mapDict[key] = Compiler(CreateMapExpression(key, MapType.Map));
                    _mapToTargetDict[key] = Compiler(CreateMapExpression(key, MapType.MapToTarget));
                }
                catch (Exception ex)
                {
                    if (failFast)
                    {
                        throw;
                    }

                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException(exceptions);
            }
        }

        /// <summary>
        /// Validates and cache mapping instructions for queryable.
        /// </summary>
        public void CompileProjection()
        {
            var keys = RuleMap.Keys.ToList();
            foreach (var key in keys)
            {
                _projectionDict[key] = CreateProjectionCallExpression(key);
            }
        }


        public IEnumerator<TypeAdapterRule> GetEnumerator()
        {
            foreach (var x in AdapterRules) yield return x;
            if (DefaultSettingsRule is not null) yield return DefaultSettingsRule;
            foreach (var x in CustomSettingsRules) yield return x;
            foreach (var x in RuleMap.Values) yield return x;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        Expression ITypeAdapterConfigSnapshot.CreateMapInvokeExpressionBody(Type sourceType, Type destinationType, Expression p)
        {
            return CreateMapInvokeExpressionBody(sourceType, destinationType, p);
        }

        Expression ITypeAdapterConfigSnapshot.CreateDynamicMapInvokeExpressionBody(Type destinationType, Expression p1)
        {
            return CreateDynamicMapInvokeExpressionBody(destinationType, p1);
        }

        Expression ITypeAdapterConfigSnapshot.CreateMapToTargetInvokeExpressionBody(Type sourceType, Type destinationType, Expression p1, Expression p2)
        {
            return CreateMapToTargetInvokeExpressionBody(sourceType, destinationType, p1, p2);
        }

        LambdaExpression ITypeAdapterConfigSnapshot.CreateInlineMapExpression(Type sourceType, Type destinationType, MapType mapType, CompileContext context, MemberMapping? mapping)
        {
            return CreateInlineMapExpression(sourceType, destinationType, mapType, context, mapping);
        }

        public int Count => DefaultSettingsRule is not null ? 1 : 0
                            + AdapterRules.Count
                            + CustomSettingsRules.Count
                            + RuleMap.Count;

        ITypeAdapterConfig ITypeAdapterConfigSnapshot.Config { get => Config; init => Config = value; }
        ConcurrentDictionary<Type, MappingTypeModel> ITypeAdapterConfigSnapshot.TypeMemberModelsCache { get => TypeMemberModelsCache; init => TypeMemberModelsCache = value; }
        ConcurrentDictionary<TypeTuple, CompiledSettingsCacheItem> ITypeAdapterConfigSnapshot.CacheCompiledTypesSettings { get => CacheCompiledTypesSettings; init => CacheCompiledTypesSettings = value; }
        ImmutableDictionary<string, bool> ITypeAdapterConfigSnapshot.BoolSettings { get => BoolSettings; init => BoolSettings = value; }
    }
}
