using Mapster.Models;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Mapster.TypeAdapterConfig
{
    public class ConfigCompileStorage
    {
        private readonly ConcurrentDictionary<TypeTuple, Delegate> _mapDict = new ConcurrentDictionary<TypeTuple, Delegate>();
        private readonly ConcurrentDictionary<TypeTuple, Delegate> _mapToTargetDict = new ConcurrentDictionary<TypeTuple, Delegate>();
        private readonly ConcurrentDictionary<TypeTuple, MethodCallExpression> _projectionDict = new ConcurrentDictionary<TypeTuple, MethodCallExpression>();
        private readonly ConcurrentDictionary<TypeTuple, Delegate> _dynamicMapDict = new ConcurrentDictionary<TypeTuple, Delegate>();
        public Func<TSource, TDestination> GetMapFunction<TSource, TDestination>()
        {
            return (Func<TSource, TDestination>)GetMapFunction(typeof(TSource), typeof(TDestination));
        }
        internal Delegate GetMapFunction(Type sourceType, Type destinationType)
        {
            var key = new TypeTuple(sourceType, destinationType);
            if (!_mapDict.TryGetValue(key, out var del))
                del = AddToHash(_mapDict, key, tuple => Compiler(CreateMapExpression(tuple, MapType.Map)));
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
                del = AddToHash(_mapToTargetDict, key, tuple => Compiler(CreateMapExpression(tuple, MapType.MapToTarget)));
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
                del = AddToHash(_dynamicMapDict, key, tuple => Compiler(CreateDynamicMapExpression(tuple)));
            return (Func<object, TDestination>)del;
        }

        private T AddToHash<T>(ConcurrentDictionary<TypeTuple, T> hash, TypeTuple key, Func<TypeTuple, T> func)
        {
            return hash.GetOrAdd(key, types =>
            {
                var del = func(types);
                hash[types] = del;

                if (RuleMap.TryGetValue(types, out var rule))
                    rule.Settings.Compiled = true;
                return del;

            });
        }
    }
}
