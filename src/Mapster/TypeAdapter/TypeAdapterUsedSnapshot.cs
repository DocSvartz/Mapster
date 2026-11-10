using Mapster.Cache;
using Mapster.Models;
using Mapster.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace Mapster
{
    public static class TypeAdapterUsedSnapshot
    {
        private static TDestination UpdateFuncFromPackedinObject<TSource, TDestination>(TSource source, TDestination destination, TypeAdapterConfigSnapshot config, Type sourceType, Type destinationType)
        {
            var del = config.GetMapToTargetFunction(sourceType, destinationType);
            var canUseDynamic = sourceType.GetTypeInfo().IsVisible && destinationType.GetTypeInfo().IsVisible;
            return DelegateInvokeCompat.InvokeMapToTarget<TDestination>(del, source!, destination!, canUseDynamic);
        }


        /// <summary>
        /// Adapt the source object to the destination type.
        /// </summary>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="config">Configuration</param>
        /// <returns>Adapted destination type.</returns>
        [return: NotNullIfNotNull(nameof(source))]
        public static TDestination? Adapt<TDestination>(this object? source, TypeAdapterConfigSnapshot config)
        {
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            Type? type;

            if (source == null)
                type = typeof(Object);
            else
                type = source.GetType();

            var fn = config.GetDynamicMapFunction<TDestination>(type);
            return fn(source)!;
        }

        /// <summary>
        /// Adapt the source object to the destination type.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="config">Configuration</param>
        /// <returns>Adapted destination type.</returns>
        public static TDestination Adapt<TSource, TDestination>(this TSource source, TypeAdapterConfigSnapshot config)
        {
            var fn = config.GetMapFunction<TSource, TDestination>();
            return fn(source);
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
        public static TDestination Adapt<TSource, TDestination>(this TSource source, TDestination destination, TypeAdapterConfigSnapshot config)
        {
            var sourceType = source?.GetType();
            var destinationType = destination?.GetType();

            if (sourceType == typeof(object)) // Infinity loop in ObjectAdapter if Runtime Type of source is Object 
                return destination;

            if (typeof(TSource) == typeof(object) || typeof(TDestination) == typeof(object))
                if (sourceType != null && destinationType != null)
                    return UpdateFuncFromPackedinObject(source, destination, config, sourceType, destinationType);

            var fn = config.GetMapToTargetFunction<TSource, TDestination>();
            return fn(source, destination);
        }

        /// <summary>
        /// Adapt the source object to the destination type.
        /// </summary>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="sourceType">The type of the source object.</param>
        /// <param name="destinationType">The type of the destination object.</param>
        /// <param name="config">Configuration</param>
        /// <returns>Adapted destination type.</returns>
        public static object? Adapt(this object source, Type sourceType, Type destinationType, TypeAdapterConfigSnapshot config)
        {
            var del = config.GetMapFunction(sourceType, destinationType);
            var canUseDynamic = sourceType.GetTypeInfo().IsVisible && destinationType.GetTypeInfo().IsVisible;
            //NOTE: if type is non-public, we cannot use dynamic
            //DynamicInvoke is slow, but works with non-public
            return DelegateInvokeCompat.InvokeMap(del, source, canUseDynamic);
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
        public static object? Adapt(this object source, object destination, Type sourceType, Type destinationType, TypeAdapterConfigSnapshot config)
        {
            var del = config.GetMapToTargetFunction(sourceType, destinationType);
            var canUseDynamic = sourceType.GetTypeInfo().IsVisible && destinationType.GetTypeInfo().IsVisible;
            //NOTE: if type is non-public, we cannot use dynamic
            //DynamicInvoke is slow, but works with non-public
            return DelegateInvokeCompat.InvokeMapToTarget(del, source, destination, canUseDynamic);
        }

        /// <summary>
        /// Validate properties with configuration and Adapt the source object to the destination type.
        /// </summary>
        /// <typeparam name="TSource">Source type.</typeparam>
        /// <typeparam name="TDestination">Destination type.</typeparam>
        /// <param name="source">Source object to adapt.</param>
        /// <param name="config">Configuration</param>
        /// <returns>Adapted destination type.</returns>
        public static TDestination ValidateAndAdapt<TSource, TDestination>(this TSource source, TypeAdapterConfigSnapshot config)
        {
            var sourceType = typeof(TSource);
            var selectorType = typeof(TDestination);

            var sourceProperties = new HashSet<string>(sourceType.GetProperties().Select(p => p.Name));
            var selectorProperties = new HashSet<string>(selectorType.GetProperties().Select(p => p.Name));

            // Get the rule map for the current types
            var ruleMap = config.RuleMap;
            var typeTuple = new TypeTuple(sourceType, selectorType);
            ruleMap.TryGetValue(typeTuple, out var rule);

            foreach (var selectorProperty in selectorProperties)
            {
                if (sourceProperties.Contains(selectorProperty)) continue;
                // Check whether the adapter config has a config for the property
                if (rule != null && rule.Settings.Resolvers.Any(r => r.DestinationMemberName.Equals(selectorProperty))) continue;
                throw new Exception($"Property {selectorProperty} does not exist in {sourceType.Name} and is not configured in Mapster");
            }
            return source.Adapt<TDestination>(config);
        }
    }
}
