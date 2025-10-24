using Mapster.Models;
using Mapster.Utils;
using System;
using System.Linq;

namespace Mapster
{
    public static class TypeAdapterConfigTSetterExtentions
    {
        /// <summary>
        /// Configures a mapping for a specific source and destination type pair.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <returns></returns>
        public static TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>(this ITypeAdapterConfig config)
        {
            var key = new TypeTuple(typeof(TSource), typeof(TDestination));
            var settings = config.GetSettings(key);
            return new TypeAdapterSetter<TSource, TDestination>(settings, config);
        }

        /// <summary>
        /// Creates a new configuration for mapping between source and destination types.
        /// </summary>
        /// <param name="sourceType">Source type to create new configuration.</param>
        /// <param name="destinationType">Destination type to create new configuration.</param>
        /// <returns></returns>
        public static TypeAdapterSetter NewConfig(this ITypeAdapterConfig config, Type sourceType, Type destinationType)
        {
            config.Remove(sourceType, destinationType);
            return config.ForType(sourceType, destinationType);
        }

        /// <summary>
        /// Creates a new configuration for mapping between source and destination types.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <returns></returns>
        public static TypeAdapterSetter<TSource, TDestination> NewConfig<TSource, TDestination>(this ITypeAdapterConfig config) 
        {
            config.Remove(typeof(TSource), typeof(TDestination));
            return config.ForType<TSource, TDestination>();
        }

        /// <summary>
        /// Configures a mapping for a specific destination type.
        /// </summary>
        /// <param name="destinationType">Destination type.</param>
        /// <returns></returns>
        public static TypeAdapterSetter ForDestinationType(this ITypeAdapterConfig config, Type destinationType)
        {
            var key = new TypeTuple(typeof(void), destinationType);
            var settings = config.GetSettings(key);
            return new TypeAdapterSetter(settings, config);
        }

        /// <summary>
        /// Configures a mapping for a specific destination type.
        /// </summary>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <returns></returns>
        public static TypeAdapterSetter<TDestination> ForDestinationType<TDestination>(this ITypeAdapterConfig config)
        {
            var key = new TypeTuple(typeof(void), typeof(TDestination));
            var settings = config.GetSettings(key);
            return new TypeAdapterSetter<TDestination>(settings, config);
        }

        /// <summary>
        /// allows you to specify conditions for when a mapping should occur based on PreCompileArgument delegate
        /// </summary>
        /// <param name="canMap"></param>
        /// <returns></returns>
        public static TypeAdapterSetter When(this ITypeAdapterConfig config, Func<PreCompileArgument, bool> canMap)
        {
            var rule = new TypeAdapterRule
            {
                Priority = arg => canMap(arg) ? (int?)25 : null,
                Settings = new TypeAdapterSettings(),
            };
            config.Rules.LockAdd(rule);
            return new TypeAdapterSetter(rule.Settings, config);
        }

        /// <summary>
        /// allows you to specify conditions for when a mapping should occur based on source and destination types and the mapping type.
        /// </summary>
        /// <param name="canMap"></param>
        /// <returns></returns>
        public static TypeAdapterSetter When(this ITypeAdapterConfig config, Func<Type, Type, MapType, bool> canMap)
        {
            var rule = new TypeAdapterRule
            {
                Priority = arg => canMap(arg.SourceType, arg.DestinationType, arg.MapType) ? (int?)25 : null,
                Settings = new TypeAdapterSettings(),
            };
            config.Rules.LockAdd(rule);
            return new TypeAdapterSetter(rule.Settings, config);
        }

        public static TypeAdapterSetter Default(this ITypeAdapterConfig config)
        {
            var arg = new PreCompileArgument() { DestinationType = typeof(void), SourceType = typeof(void), MapType = MapType.Map };
            var settings = config.Rules.Where(x => x.Priority.Invoke(arg) == -100).First().Settings;

            return new TypeAdapterSetter(settings, config);
        }
    }
}
