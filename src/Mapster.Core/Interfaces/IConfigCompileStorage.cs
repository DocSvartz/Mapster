using System;

namespace Mapster
{
    public interface IConfigCompileStorage
    {
        Func<object, TDestination> GetDynamicMapFunction<TDestination>(Type sourceType);
        Delegate GetMapFunction(Type sourceType, Type destinationType);
        Func<TSource, TDestination> GetMapFunction<TSource, TDestination>();
        Func<TSource, TDestination, TDestination> GetMapToTargetFunction<TSource, TDestination>();
    }
}