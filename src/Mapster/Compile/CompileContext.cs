using Mapster.Cache;
using Mapster.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapster
{
    public class CompileContext
    {
        public bool IsExplicitMapping (TypeTuple types) => ConfigSnapshot.RuleMap.ContainsKey(types);
        public HashSet<TypeTuple> Running { get; } = new();
        public Stack<TypeAdapterConfigSnapshot> Configs { get; } = new();
        public TypeAdapterConfigSnapshot ConfigSnapshot => Configs.Peek();
        public int? MaxDepth { get; set; }
        public int Depth { get; set; }
        public HashSet<ParameterExpression> ExtraParameters { get; } = new();
               
        internal AttributeMetadataCache AttributeMetadata { get; } = new();

        internal bool IsSubFunction()
        {
            return MaxDepth.HasValue || ExtraParameters.Count > 0;
        }
        public CompileContext(TypeAdapterConfigSnapshot snapshot)
        {
            Configs.Push(snapshot);
        }
        public CompileContext(TypeAdapterConfig config)
        {
            //Configs.Push(config);
        }

        internal TypeAdapterSettings GetMergedSettings(TypeTuple tuple, MapType mapType)
        {
            // Get settings from cache. If found skip apply rules and create rule and setting from attibutes
            if (TryGetCachedSettings(tuple, mapType, out var foundsettings, out var cacheItem))
                return foundsettings;


            var arg = new PreCompileArgument
            {
                SourceType = tuple.Source,
                DestinationType = tuple.Destination,
                MapType = mapType,
                ExplicitMapping = ConfigSnapshot.RuleMap.ContainsKey(tuple),
                TypeMemberModelsCache = ConfigSnapshot.TypeMemberModelsCache

            };

            ConfigSnapshot.TypeMemberModelsCache.TryGetValue(tuple.Source, out var srcTypeModel);
            ConfigSnapshot.TypeMemberModelsCache.TryGetValue(tuple.Destination, out var destTypeModel);

            var result = new CompiledTypesSettings(tuple);
            IEnumerable<TypeAdapterRule> attrSettings = Enumerable.Empty<TypeAdapterRule>();
            bool IsHasAttributeRule = false;

            if (srcTypeModel is null || destTypeModel is null)
            {
                if (srcTypeModel is null)
                {
                    srcTypeModel = tuple.Source.GetMappingTypeModel();
                    ConfigSnapshot.TypeMemberModelsCache[tuple.Source] = srcTypeModel;
                }
                if(destTypeModel is null)
                {
                    destTypeModel = tuple.Destination.GetMappingTypeModel();
                    ConfigSnapshot.TypeMemberModelsCache[tuple.Destination] = destTypeModel;
                }

            }
            else
            {
                //auto add setting if there is attr setting
                attrSettings = GetAttributeSettings(tuple, mapType, srcTypeModel, destTypeModel).ToList();
                if (!arg.ExplicitMapping && attrSettings.Any(rule => rule.Priority(arg) == 100))
                {
                    //GetSettings(tuple);
                    arg.ExplicitMapping = true;
                }
            }

            if (attrSettings?.Any() == true)
                IsHasAttributeRule = true;
                       
             
            var rules = ConfigSnapshot.Rules.Concat(attrSettings);
                
            var settings = from rule in rules
                               let priority = rule.Priority(arg)
                               where priority != null
                               orderby priority.Value descending
                               select rule.Settings;
                foreach (var setting in settings)
                {
                    result.Apply(setting);
                }

            // Apply Inherits settings

            if (result.Inherits.Any())
            {
                List<TypeAdapterRule> inherits = new(result.Inherits.Count);
                
                foreach (var types in result.Inherits)
                {
                    ConfigSnapshot.RuleMap.TryGetValue(types, out var rule);

                    if (rule != null)
                        inherits.Add(rule);
                }

                foreach (var item in inherits)
                {
                    result.Apply(item.Settings);
                }
            }
                      

            //remove recursive include types
            if (mapType == MapType.MapToTarget)
                result.Includes.Remove(tuple);
            else
                result.Includes.RemoveAll(t => t.Source == tuple.Source);

            // Update cache
            UpdateCachedSettings(tuple, IsHasAttributeRule, cacheItem, result, arg);

            //Defining the parameters of the mapping function
            result.Source = Expression.Parameter(tuple.Source, "p1");
            result.Destination = Expression.Parameter(tuple.Destination, "p2");
            result.Result = Expression.Parameter(tuple.Destination, "result");

            return result;
        }

        private IEnumerable<TypeAdapterRule> GetAttributeSettings(TypeTuple tuple, MapType mapType, 
            MappingTypeModel srcTypeModel, MappingTypeModel destTypeModel)
        {
            var rules1 = from o in srcTypeModel.CustomAttributes
                         where typeof(AdaptToAttribute).IsAssignableFrom(o.GetAttributeType())
                         let attr = o.CreateCustomAttribute<AdaptToAttribute>()
                         where attr != null && (attr.MapType & mapType) != 0
                         where attr.Type == null || attr.Type == tuple.Destination
                         where attr.Name == null || attr.Name.Replace("[name]", srcTypeModel.Type.Name) == tuple.Destination.Name
                         let distance = tuple.Source.GetSubclassDistance(srcTypeModel.Type, true)
                         select new TypeAdapterRule((arg => distance + 50), CreateSettings(attr));
            if (tuple.Source == tuple.Destination)
                return rules1;
            var rules2 = from o in destTypeModel.CustomAttributes
                         where typeof(AdaptFromAttribute).IsAssignableFrom(o.GetAttributeType()) ||
                               typeof(AdaptTwoWaysAttribute).IsAssignableFrom(o.GetAttributeType())
                         let attr = o.CreateCustomAttribute<BaseAdaptAttribute>()
                         where attr != null && (attr.MapType & mapType) != 0
                         where attr.Type == null || attr.Type == tuple.Source
                         where attr.Name == null || attr.Name.Replace("[name]", destTypeModel.Type.Name) == tuple.Source.Name
                         let distance = tuple.Destination.GetSubclassDistance(destTypeModel.Type, true)
                         select new TypeAdapterRule((arg => distance + 50), CreateSettings(attr));
                         
            return rules1.Concat(rules2);
        }

        private TypeAdapterSettings CreateSettings(BaseAdaptAttribute attr)
        {
            //Finalize settings are used here, since the consumer will not receive
            //it in an intermediate state—unlike in NewConfig() or ForType() creation.
            var settings = TypeAdapterSettingsFactory.GetSettings;
            var setter = new TypeAdapterSetter(settings, ConfigSnapshot.Config);
            setter.ApplyAdaptAttribute(attr);
            return settings.ToReadOnlyAndDisposeThis();
        }

        /// <summary>
        /// Attempts to retrieve cached compilation settings for the specified type pair and mapping type.
        /// </summary>
        /// <param name="tuple">The type tuple used as the primary key in the cache.</param>
        /// <param name="mapType">The mapping type for which compiled settings are requested.</param>
        /// <param name="settings">When this method returns, contains the cached compiled settings if found; otherwise, null.</param>
        /// <param name="cacheItem">When this method returns, contains the cache item associated with the type tuple if found; otherwise, null.</param>
        /// <returns>
        /// Returns <c>true</c> if both the cache item and the corresponding compiled settings were successfully retrieved; 
        /// otherwise, returns <c>false</c>.
        /// </returns>
        private bool TryGetCachedSettings(TypeTuple tuple, MapType mapType, [NotNullWhen(true)] out CompiledTypesSettings? settings, out CompiledSettingsCacheItem? cacheItem)
        {
            ConfigSnapshot.CacheCompiledTypesSettings.TryGetValue(tuple, out cacheItem);
            if(cacheItem != null)
            {
                cacheItem.TryGetValue(mapType, out settings);
                if(settings != null)
                    return true;
            }
            else
                settings = null;

            return false;
        }

        private void UpdateCachedSettings(TypeTuple tuple, bool IsHasAttributeRule, CompiledSettingsCacheItem cacheItem, CompiledTypesSettings result, PreCompileArgument arg)
        {
            if (tuple.Source == typeof(Never) || tuple.Destination == typeof(Never))
                return; // it is special Marker type 

            CompiledSettingsCacheItem updateCache = null!;

            if (IsHasAttributeRule)
            {
                if (arg.MapType == MapType.Map)
                    updateCache = cacheItem != null ? cacheItem with { Map = result } : new CompiledSettingsCacheItem(result, null, null);
                if (arg.MapType == MapType.MapToTarget)
                    updateCache = cacheItem != null ? cacheItem with { MapToTaget = result } : new CompiledSettingsCacheItem(null, result, null);
                if (arg.MapType == MapType.Projection)
                    updateCache = cacheItem != null ? cacheItem with { Projection = result } : new CompiledSettingsCacheItem(null, null, result);
            }
            else if (arg.MapType == MapType.Projection)
            {
                // !!! For now, you always need to create a separate cache for a Projection !!!
                updateCache = cacheItem != null ? cacheItem with { Projection = result } : new CompiledSettingsCacheItem(null, null, result, false);
            }
            else
                updateCache = cacheItem != null ? cacheItem with { Map = result, MapToTaget = result } : new CompiledSettingsCacheItem(result, result, null, false);

            ConfigSnapshot.CacheCompiledTypesSettings[tuple] = updateCache;
        }
    }
}