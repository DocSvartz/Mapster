using System;

namespace Mapster
{
    public static class TypeAdapterBase<TConfig, TSource, TDestination> 
        where TConfig : ITypeAdapterConfig, new()
    {
        public static Func<TSource, TDestination> Map;

        public static Func<TSource, TDestination> MapToTarget;
    }

    internal static class TypeAdapterProjectionBase<TConfig, TSource, TDestination>
        where TConfig : ITypeAdapterConfig, new()
    {
       //
    }

    public static class FrozenTypeAdapter<TConfig,TSource, TDestination>
        where TConfig : FrozenTypeAdapterConfig
    {
        public static Func<TSource, TDestination> Map;

        public static Func<TSource, TDestination> MapToTarget;
    }

    internal static class FrozenTypeAdapterProjection<TConfig, TSource, TDestination>
        where TConfig : FrozenTypeAdapterConfig
    {
        //
    }
}
