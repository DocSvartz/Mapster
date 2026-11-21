using Mapster.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mapster.TypeAdapterConfigs.ConfigSnapshots
{
    public abstract class BaseNotStrictConfigSnapshot : BaseConfigSnapshot
    {
        #region CacheFunc
        internal readonly ConcurrentDictionary<TypeTuple, Delegate> _mapDict = new();
        internal readonly ConcurrentDictionary<TypeTuple, Delegate> _mapToTargetDict = new();
        internal readonly ConcurrentDictionary<TypeTuple, MethodCallExpression> _projectionDict = new();
        internal readonly ConcurrentDictionary<TypeTuple, Delegate> _dynamicMapDict = new();

        protected override Delegate AddOrGetMapFuncFromCache(TypeTuple tuple, bool forceAdd = false)
        {
            if (forceAdd)
            {
                var del = Compiler(CreateMapExpression(tuple, MapType.Map));
                _mapDict[tuple] = del;
                return del;
            }
            else
            {
                if (!_mapDict.TryGetValue(tuple, out var del))
                    del = AddToHash(_mapDict, tuple, tuple => Compiler(CreateMapExpression(tuple, MapType.Map)));

                return del;
            }

        }

        protected override Delegate AddOrGetMapToTargetFuncFromCache(TypeTuple tuple, bool forceAdd = false)
        {
            if (forceAdd)
            {
                var del = Compiler(CreateMapExpression(tuple, MapType.MapToTarget));
                _mapToTargetDict[tuple] = del;
                return del;
            }
            else
            {
                if (!_mapToTargetDict.TryGetValue(tuple, out var del))
                    del = AddToHash(_mapToTargetDict, tuple, tuple => Compiler(CreateMapExpression(tuple, MapType.MapToTarget)));

                return del;
            }
        }

        protected override MethodCallExpression AddOrGetProjectionFuncFromCache(TypeTuple tuple, bool forceAdd = false)
        {
            if (forceAdd)
            {
                var del = CreateProjectionCallExpression(tuple);
                _projectionDict[tuple] = del;
                return del;
            }
            else
            {
                if (!_projectionDict.TryGetValue(tuple, out var del))
                    del = AddToHash(_projectionDict, tuple, CreateProjectionCallExpression);

                return del;
            }
        }

        protected override Func<object, TDestination> AddOrGetGetDynamicMapFunction<TDestination>(Type sourceType, bool forceAdd = false)
        {
            var key = new TypeTuple(sourceType, typeof(TDestination));

            if (forceAdd)
            {
                var del = Compiler(CreateDynamicMapExpression(key));
                _dynamicMapDict[key] = del;
                return (Func<object, TDestination>)del;
            }
            else
            {
                if (!_dynamicMapDict.TryGetValue(key, out var del))
                    del = AddToHash(_dynamicMapDict, key, tuple => Compiler(CreateDynamicMapExpression(tuple)));

                return (Func<object, TDestination>)del;
            }
        }

        #endregion CacheFunc

        #region Compile

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

        #endregion Compile


    }
}
