using Mapster.Models;
using Mapster.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapster
{
    public class ConfigCompileStorage
    {

        private readonly ITypeAdapterConfig _config;

        private readonly ConcurrentDictionary<TypeTuple, Delegate> _mapDict = new ConcurrentDictionary<TypeTuple, Delegate>();
        private readonly ConcurrentDictionary<TypeTuple, Delegate> _mapToTargetDict = new ConcurrentDictionary<TypeTuple, Delegate>();
        private readonly ConcurrentDictionary<TypeTuple, MethodCallExpression> _projectionDict = new ConcurrentDictionary<TypeTuple, MethodCallExpression>();
        private readonly ConcurrentDictionary<TypeTuple, Delegate> _dynamicMapDict = new ConcurrentDictionary<TypeTuple, Delegate>();
        
        public ConfigCompileStorage(ITypeAdapterConfig config)
        {
            _config = config;
        }

        public Func<TSource, TDestination> GetMapFunction<TSource, TDestination>()
        {
            return (Func<TSource, TDestination>)GetMapFunction(typeof(TSource), typeof(TDestination));
        }
        public Delegate GetMapFunction(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            if (!_mapDict.TryGetValue(key, out var del))
                del = AddToHash(_mapDict, key, tuple => _config.Compiler(_config.CreateMapExpression(tuple, MapType.Map)));
            return del;
        }

        
        public Func<TSource, TDestination, TDestination> GetMapToTargetFunction<TSource, TDestination>()
        {
            return (Func<TSource, TDestination, TDestination>)GetMapToTargetFunction(typeof(TSource), typeof(TDestination));
        }
        internal Delegate GetMapToTargetFunction(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            if (!_mapToTargetDict.TryGetValue(key, out var del))
                del = AddToHash(_mapToTargetDict, key, tuple => _config.Compiler(_config.CreateMapExpression(tuple, MapType.MapToTarget)));
            return del;
        }

        
        internal Expression<Func<TSource, TDestination>> GetProjectionExpression<TSource, TDestination>()
        {
            var del = GetProjectionCallExpression(typeof(TSource), typeof(TDestination));

            return (Expression<Func<TSource, TDestination>>)((UnaryExpression)del.Arguments[1]).Operand;
        }
        internal MethodCallExpression GetProjectionCallExpression(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            if (!_projectionDict.TryGetValue(key, out var del))
                del = AddToHash(_projectionDict, key, CreateProjectionCallExpression);
            return del;
        }

        
        public Func<object, TDestination> GetDynamicMapFunction<TDestination>(Type sourceType)
        {
            var key = new TypeTuple(sourceType, typeof(TDestination));
            if (!_dynamicMapDict.TryGetValue(key, out var del))
                del = AddToHash(_dynamicMapDict, key, tuple => _config.Compiler(CreateDynamicMapExpression(tuple)));
            return (Func<object, TDestination>)del;
        }

        private T AddToHash<T>(ConcurrentDictionary<TypeTuple, T> hash, TypeTuple key, Func<TypeTuple, T> func)
        {
            return hash.GetOrAdd(key, types =>
            {
                var del = func(types);
                hash[types] = del;

                if (_config.RuleMap.TryGetValue(types, out var rule))
                    rule.Settings.Compiled = true;
                return del;

            });
        }

        private LambdaExpression CreateDynamicMapExpression(TypeTuple tuple)
        {
            var lambda = _config.CreateMapExpression(tuple, MapType.Map);
            var pNew = Expression.Parameter(typeof(object));
            var pOld = lambda.Parameters[0];
            var assign = ExpressionEx.Assign(pOld, pNew);
            return Expression.Lambda(
                Expression.Block(new[] { pOld }, assign, lambda.Body),
                pNew);
        }

        private MethodCallExpression CreateProjectionCallExpression(TypeTuple tuple)
        {
            var lambda = _config.CreateMapExpression(tuple, MapType.Projection);
            var source = Expression.Parameter(typeof(IQueryable<>).MakeGenericType(tuple.Source));
            var methodInfo = (from method in typeof(Queryable).GetMethods()
                              where method.Name == nameof(Queryable.Select)
                              let p = method.GetParameters()[1]
                              where p.ParameterType.GetGenericArguments()[0].GetGenericTypeDefinition() == typeof(Func<,>)
                              select method).First().MakeGenericMethod(tuple.Source, tuple.Destination);
            return Expression.Call(methodInfo, source, Expression.Quote(lambda));
        }


        internal Expression CreateMapInvokeExpressionBody(Type sourceType, Type destinationType, Expression p)
        {
            if (_config.RequireExplicitMapping || _config.RequireExplicitMappingPrimitive)
            {
                var key = new TypeTuple(sourceType, destinationType);
                _mapDict[key] = _config.Compiler(_config.CreateMapExpression(key, MapType.Map));
            }
            Expression invoker;
            if (_config.IsGlobalSettings)
            {
                var field = typeof(TypeAdapter<,>).MakeGenericType(sourceType, destinationType).GetField("Map");
                invoker = Expression.Field(null, field);
            }
            else
            {
                var method = (from m in typeof(ConfigCompileStorage).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                              where m.Name == nameof(GetMapFunction)
                              select m).First().MakeGenericMethod(sourceType, destinationType);
                invoker = Expression.Call(CreateSelfExpression(), method);
            }
            return Expression.Call(invoker, "Invoke", null, p);
        }

        internal Expression CreateMapToTargetInvokeExpressionBody(Type sourceType, Type destinationType, Expression p1, Expression p2)
        {
            if (_config.RequireExplicitMapping || _config.RequireExplicitMappingPrimitive)
            {
                var key = new TypeTuple(sourceType, destinationType);
                _mapToTargetDict[key] = _config.Compiler(_config.CreateMapExpression(key, MapType.MapToTarget));
            }
            var method = (from m in typeof(ConfigCompileStorage).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                          where m.Name == nameof(GetMapToTargetFunction)
                          select m).First().MakeGenericMethod(sourceType, destinationType);
            var invoker = Expression.Call(CreateSelfExpression(), method);
            return Expression.Call(invoker, "Invoke", null, p1, p2);
        }

        private Expression CreateSelfExpression()
        {
            var s = typeof(TypeAdapterConfigFactory).GetProperty(nameof(TypeAdapterConfigFactory.GlobalSettings));

            if (_config.IsGlobalSettings)
                return Expression.Property(null, typeof(TypeAdapterConfigFactory).GetProperty(nameof(TypeAdapterConfigFactory.GlobalSettings.ConfigCompile))!);
            else
                return Expression.Constant(this);
        }

        internal Expression CreateDynamicMapInvokeExpressionBody(Type destinationType, Expression p1)
        {
            var method = (from m in typeof(ConfigCompileStorage).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                          where m.Name == nameof(GetDynamicMapFunction)
                          select m).First().MakeGenericMethod(destinationType);
            var getType = typeof(object).GetMethod(nameof(GetType));
            var invoker = Expression.Call(CreateSelfExpression(), method, Expression.Call(p1, getType!));
            return Expression.Call(invoker, "Invoke", null, p1);
        }

        internal LambdaExpression CreateInlineMapExpression(Type sourceType, Type destinationType, MapType mapType, CompileContext context, MemberMapping? mapping = null)
        {
            var tuple = new TypeTuple(sourceType, destinationType);
            var subFunction = context.IsSubFunction();

            if (!subFunction)
            {
                if (context.Running.Contains(tuple))
                {
                    if (mapType == MapType.Projection)
                        throw new InvalidOperationException("Projection does not support circular reference, please use MaxDepth setting");
                    return CreateMapInvokeExpression(sourceType, destinationType, mapType);
                }
                context.Running.Add(tuple);
            }

            try
            {
                var arg = GetCompileArgument(tuple, mapType, context);
                if (mapping != null)
                {
                    arg.Settings.Resolvers.AddRange(mapping.NextResolvers);
                    arg.Settings.Ignore.Apply(mapping.NextIgnore);
                    arg.UseDestinationValue = mapping.UseDestinationValue;
                }

                return arg.CreateMapExpression();
            }
            finally
            {
                if (!subFunction)
                    context.Running.Remove(tuple);
            }
        }

        private CompileArgument GetCompileArgument(TypeTuple tuple, MapType mapType, CompileContext context)
        {
            var setting = _config.GetMergedSettings(tuple, mapType);
            return new CompileArgument
            {
                SourceType = tuple.Source,
                DestinationType = tuple.Destination,
                ExplicitMapping = _config.RuleMap.ContainsKey(tuple),
                MapType = mapType,
                Context = context,
                Settings = setting,
            };
        }

        internal LambdaExpression CreateMapInvokeExpression(Type sourceType, Type destinationType, MapType mapType)
        {
            return mapType == MapType.MapToTarget
                ? CreateMapToTargetInvokeExpression(sourceType, destinationType)
                : CreateMapInvokeExpression(sourceType, destinationType);
        }

        private LambdaExpression CreateMapInvokeExpression(Type sourceType, Type destinationType)
        {
            var p = Expression.Parameter(sourceType);
            var invoke = CreateMapInvokeExpressionBody(sourceType, destinationType, p);
            return Expression.Lambda(invoke, p);
        }


        private LambdaExpression CreateMapToTargetInvokeExpression(Type sourceType, Type destinationType)
        {
            var p1 = Expression.Parameter(sourceType);
            var p2 = Expression.Parameter(destinationType);
            var invoke = CreateMapToTargetInvokeExpressionBody(sourceType, destinationType, p1, p2);
            return Expression.Lambda(invoke, p1, p2);
        }

        /// <summary>
        /// Validates and cache mapping instructions.
        /// </summary>
        /// <param name="failFast">A boolean parameter that determines whether exceptions should be thrown immediately when mapping errors occur or whether to collect and aggregate them. The default value is true.</param>
        /// <exception cref="AggregateException"></exception>
        public void Compile(bool failFast = true)
        {
            var exceptions = new List<Exception>();
            var keys = _config.RuleMap.Keys.ToList();

            foreach (var key in keys)
            {
                try
                {
                    if (key.Source == typeof(void))
                        continue;

                    _mapDict[key] = _config.Compiler(_config.CreateMapExpression(key, MapType.Map));
                    _mapToTargetDict[key] = _config.Compiler(_config.CreateMapExpression(key, MapType.MapToTarget));
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
        /// Validates and cache mapping instructions.
        /// </summary>
        /// <param name="sourceType">Source type to compile.</param>
        /// <param name="destinationType">Destination type to compile.</param>
        public void Compile(Type sourceType, Type destinationType)
        {
            var tuple = new TypeTuple(sourceType, destinationType);
            _mapDict[tuple] = _config.Compiler(_config.CreateMapExpression(tuple, MapType.Map));
            _mapToTargetDict[tuple] = _config.Compiler(_config.CreateMapExpression(tuple, MapType.MapToTarget));
            if (_config.IsGlobalSettings)
            {
                var field = typeof(TypeAdapter<,>).MakeGenericType(sourceType, destinationType).GetField("Map");
                field!.SetValue(null, _mapDict[tuple]);
            }
        }

        /// <summary>
        /// Validates and cache mapping instructions for queryable.
        /// </summary>
        public void CompileProjection()
        {
            var keys = _config.RuleMap.Keys.ToList();
            foreach (var key in keys)
            {
                _projectionDict[key] =  CreateProjectionCallExpression(key);
            }
        }

        /// <summary>
        /// Validates and cache mapping instructions for queryable.
        /// </summary>
        /// <param name="sourceType">Source type to compile.</param>
        /// <param name="destinationType">Destination type to compile.</param>
        public void CompileProjection(Type sourceType, Type destinationType)
        {
            var tuple = new TypeTuple(sourceType, destinationType);
            _projectionDict[tuple] = CreateProjectionCallExpression(tuple);
        }

        internal void Remove(TypeTuple key)
        {
            _mapDict.TryRemove(key, out _);
            _mapToTargetDict.TryRemove(key, out _);
            _projectionDict.TryRemove(key, out _);
            _dynamicMapDict.TryRemove(key, out _);
        }
    }
}
