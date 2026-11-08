using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Mapster.Models;
using Mapster.Utils;

namespace Mapster
{
    public static class TypeAdapter
    {
        public static ITypeAdapterBuilder<TSource> BuildAdapter<TSource>(this TSource source)
        {
            return new TypeAdapterBuilder<TSource>(source, MapsterGlobalConfig.GlobalSettings);
        }

        public static ITypeAdapterBuilder<TSource> BuildAdapter<TSource>(this TSource source, TypeAdapterConfig config)
        {
            return new TypeAdapterBuilder<TSource>(source, config);
        }

        /// <summary>
        /// Adapt the source object to the destination type.
        /// </summary>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <returns>Adapted destination type.</returns>
        [return: NotNullIfNotNull(nameof(source))]
        public static TDestination? Adapt<TDestination>(this object? source)
        {
            return Adapt<TDestination>(source, MapsterGlobalConfig.GlobalSettings);
        }

        /// <summary>
        /// Adapt the source object to the destination type.
        /// </summary>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="config">Configuration</param>
        /// <returns>Adapted destination type.</returns>
        [return: NotNullIfNotNull(nameof(source))]
        public static TDestination? Adapt<TDestination>(this object? source, TypeAdapterConfig config)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse

            var snapshot = config.GetSnapshot;
            return source.Adapt<TDestination>(snapshot);
        }

        /// <summary>
        /// Adapt the source object to the destination type.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <returns>Adapted destination type.</returns>
        public static TDestination Adapt<TSource, TDestination>(this TSource source)
        {
            var config = MapsterGlobalConfig.GlobalSettings.GetSnapshot;
            var fn = config.GetMapFunction<TSource, TDestination>();

            return fn(source);

            //return TypeAdapter<TSource, TDestination>.Map(source);
        }

        /// <summary>
        /// Adapt the source object to the destination type.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="config">Configuration</param>
        /// <returns>Adapted destination type.</returns>
        public static TDestination Adapt<TSource, TDestination>(this TSource source, TypeAdapterConfig config)
        {
            var fn = config.GetSnapshot.GetMapFunction<TSource, TDestination>();
            return fn(source);
        }

        /// <summary>
        /// Adapt the source object to the existing destination object.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="destination">The destination object to populate.</param>
        /// <returns>Adapted destination type.</returns>
        public static TDestination Adapt<TSource, TDestination>(this TSource source, TDestination destination)
        {
            return Adapt(source, destination, MapsterGlobalConfig.GlobalSettings);
        }

        /// <summary>
        /// Adapt the source object to the existing destination object.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="destination">The destination object to populate.</param>
        /// <param name="config">Configuration</param>
        /// <returns>Adapted destination type.</returns>
        public static TDestination Adapt<TSource, TDestination>(this TSource source, TDestination destination, TypeAdapterConfig config)
        {
            var snapshot = config.GetSnapshot;
            return source.Adapt<TSource, TDestination>(destination,snapshot);
        }
                
        /// <summary>
        /// Adapt the source object to the destination type.
        /// </summary>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="sourceType">The type of the source object.</param>
        /// <param name="destinationType">The type of the destination object.</param>
        /// <returns>Adapted destination type.</returns>
        public static object? Adapt(this object source, Type sourceType, Type destinationType)
        {
            if (source != null &&
                sourceType.IsOpenGenericType() && destinationType.IsOpenGenericType())
            {
                var arg = source.GetType().GetGenericArguments();
                return Adapt(source, sourceType.MakeGenericType(arg), destinationType.MakeGenericType(arg), MapsterGlobalConfig.GlobalSettings);
            }
            return Adapt(source, sourceType, destinationType, MapsterGlobalConfig.GlobalSettings);
        }

        /// <summary>
        /// Adapt the source object to the destination type.
        /// </summary>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="sourceType">The type of the source object.</param>
        /// <param name="destinationType">The type of the destination object.</param>
        /// <param name="config">Configuration</param>
        /// <returns>Adapted destination type.</returns>
        public static object? Adapt(this object source, Type sourceType, Type destinationType, TypeAdapterConfig config)
        {
            var snapshot = config.GetSnapshot;

            return source.Adapt(sourceType, destinationType, snapshot);
        }

        /// <summary>
        /// Adapt the source object to an existing destination object.
        /// </summary>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="destination">Destination object to populate.</param>
        /// <param name="sourceType">The type of the source object.</param>
        /// <param name="destinationType">The type of the destination object.</param>
        /// <returns>Adapted destination type.</returns>
        public static object? Adapt(this object source, object destination, Type sourceType, Type destinationType)
        {
            return Adapt(source, destination, sourceType, destinationType, MapsterGlobalConfig.GlobalSettings);
        }

        /// <summary>
        /// Adapt the source object to an existing destination object.
        /// </summary>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="destination">Destination object to populate.</param>
        /// <param name="sourceType">The type of the source object.</param>
        /// <param name="destinationType">The type of the destination object.</param>
        /// <param name="config">Configuration</param>
        /// <returns>Adapted destination type.</returns>
        public static object? Adapt(this object source, object destination, Type sourceType, Type destinationType, TypeAdapterConfig config)
        {
            var snapshot = config.GetSnapshot;
            
            return source.Adapt(destination, sourceType, destinationType, snapshot);
        }
        
        /// <summary>
        /// Validate properties and Adapt the source object to the destination type.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <returns>Adapted destination type.</returns>
        public static TDestination ValidateAndAdapt<TSource, TDestination>(this TSource source)
        {
            var sourceType = typeof(TSource);
            var selectorType = typeof(TDestination);

            var sourceProperties = new HashSet<string>(sourceType.GetProperties().Select(p => p.Name));
            var selectorProperties = new HashSet<string>(selectorType.GetProperties().Select(p=> p.Name));

            foreach (var selectorProperty in selectorProperties)
            {
                if (sourceProperties.Contains(selectorProperty)) continue;
                throw new Exception($"Property {selectorProperty} does not exist in {sourceType.Name} and is not configured in Mapster");
            }
            return source.Adapt<TDestination>();
        }
        
        /// <summary>
        /// Validate properties with configuration and Adapt the source object to the destination type.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="config">Configuration</param>
        /// <returns>Adapted destination type.</returns>
        public static TDestination ValidateAndAdapt<TSource, TDestination>(this TSource source, TypeAdapterConfig config)
        {
            var snapshot = config.GetSnapshot;
            return source.ValidateAndAdapt<TSource, TDestination>(snapshot);
        }


        /// <summary>
        /// Adapt the source object to a destination type using a temporary configuration.
        /// A new TypeAdapterConfig is created for this call, ensuring GlobalSettings remain unchanged.
        /// Safe for init-only properties and record types.
        /// </summary>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="configAction">Action to customize the temporary config.</param>
        /// <returns>Adapted destination object of type TDestination.</returns>
        public static TDestination Adapt<TDestination>(this object? source, Action<TypeAdapterConfig> configAction)
        {
            var config = MapsterGlobalConfig.GlobalSettings.Clone();
            configAction(config);
            return source.Adapt<TDestination>(config);
        }

        /// <summary>
        /// Adapt the source object from TSource to TDestination using a dedicated TypeAdapterSetter.
        /// A temporary TypeAdapterConfig is created and configured via the setter.
        /// Safe for init-only properties and record types, without modifying GlobalSettings.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="configAction">Action to customize the TypeAdapterSetter.</param>
        /// <returns>Adapted destination object of type TDestination.</returns>
        public static TDestination Adapt<TSource, TDestination>(this object? source, Action<TypeAdapterSetter<TSource, TDestination>> configAction)
        {
            var config = MapsterGlobalConfig.GlobalSettings.Clone();
            var setter = config.ForType<TSource, TDestination>();
            configAction(setter);
            setter.Settings.Resolvers.Reverse();
            return source.Adapt<TDestination>(config);
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "<Pending>")]
    public static class TypeAdapter<TSource, TDestination>
    {
        public static Func<TSource, TDestination> Map = MapsterGlobalConfig.GlobalSettings.GetSnapshot.GetMapFunction<TSource, TDestination>();
    }
}
