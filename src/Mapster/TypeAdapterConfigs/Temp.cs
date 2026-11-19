using Mapster.Models;
using System;
using System.Linq.Expressions;

namespace Mapster
{
    public abstract partial class BaseMapFuncProducer
    {
        protected virtual Delegate AddOrGetMapFuncFromCache(TypeTuple tuple, bool forceAdd = false)
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

        protected virtual Delegate AddOrGetMapToTargetFuncFromCache(TypeTuple tuple, bool forceAdd = false)
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

        protected virtual MethodCallExpression AddOrGetProjectionFuncFromCache(TypeTuple tuple, bool forceAdd = false)
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

        protected virtual Func<object, TDestination> AddOrGetGetDynamicMapFunction<TDestination>(Type sourceType, bool forceAdd = false)
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

    }
}
