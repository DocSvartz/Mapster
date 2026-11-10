using Mapster.Cache;
using Mapster.Models;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
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
        //public SettingsCacheStorage MappingCache => ConfigSnapshot.MappingCache;
        public List<TypeAdapterRule> CompiledRules { get; private set; } = new();
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
            var arg = new PreCompileArgument
            {
                SourceType = tuple.Source,
                DestinationType = tuple.Destination,
                MapType = mapType,
                ExplicitMapping = ConfigSnapshot.RuleMap.ContainsKey(tuple),
                TypeMemberModelsCache = ConfigSnapshot.TypeMemberModelsCache

            };

            //auto add setting if there is attr setting
            var attrSettings = GetAttributeSettings(tuple, mapType).ToList();
            if (!arg.ExplicitMapping && attrSettings.Any(rule => rule.Priority(arg) == 100))
            {
                //GetSettings(tuple);
                arg.ExplicitMapping = true;
            }

            var result = new CompiledTypesSettings(tuple);
             
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

            CompiledRules.Add(ConfigSnapshot.Config.MappingCache.GetCompiledRule(tuple,result));

            return result;
        }

        private IEnumerable<TypeAdapterRule> GetAttributeSettings(TypeTuple tuple, MapType mapType)
        {
            var rules1 = from type in tuple.Source.GetAllTypes()
                         from o in type.GetTypeInfo().GetCustomAttributesData()
                         where typeof(AdaptToAttribute).IsAssignableFrom(o.GetAttributeType())
                         let attr = o.CreateCustomAttribute<AdaptToAttribute>()
                         where attr != null && (attr.MapType & mapType) != 0
                         where attr.Type == null || attr.Type == tuple.Destination
                         where attr.Name == null || attr.Name.Replace("[name]", type.Name) == tuple.Destination.Name
                         let distance = tuple.Source.GetSubclassDistance(type, true)
                         select new ExplicitTypeAdapterRule((arg => distance + 50), CreateSettings(attr));
            if (tuple.Source == tuple.Destination)
                return rules1;
            var rules2 = from type in tuple.Destination.GetAllTypes()
                         from o in type.GetTypeInfo().GetCustomAttributesData()
                         where typeof(AdaptFromAttribute).IsAssignableFrom(o.GetAttributeType()) ||
                               typeof(AdaptTwoWaysAttribute).IsAssignableFrom(o.GetAttributeType())
                         let attr = o.CreateCustomAttribute<BaseAdaptAttribute>()
                         where attr != null && (attr.MapType & mapType) != 0
                         where attr.Type == null || attr.Type == tuple.Source
                         where attr.Name == null || attr.Name.Replace("[name]", type.Name) == tuple.Source.Name
                         let distance = tuple.Destination.GetSubclassDistance(type, true)
                         select new ExplicitTypeAdapterRule((arg => distance + 50), CreateSettings(attr));
                         
            return rules1.Concat(rules2);
        }

        private TypeAdapterSettings CreateSettings(BaseAdaptAttribute attr)
        {
            //Finalize settings are used here, since the consumer will not receive
            //it in an intermediate state—unlike in NewConfig() or ForType() creation.
            var settings = SettingsFactory.GetExplicitTypeSettings;
            var setter = new TypeAdapterSetter(settings, ConfigSnapshot.Config);
            setter.ApplyAdaptAttribute(attr);
            return settings.Finalize();
        }

    }
}