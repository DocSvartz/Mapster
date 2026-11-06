using Mapster.Cache;
using Mapster.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Mapster
{
    public class CompileContext
    {
        public HashSet<TypeTuple> Running { get; } = new();
        public Stack<TypeAdapterConfig> Configs { get; } = new();
        public TypeAdapterConfig Config => Configs.Peek();
        public SettingsCacheStorage MappingCache => Config.MappingCache;
        public List<TypeAdapterRule> CompiledRules { get; } = new();
        public int? MaxDepth { get; set; }
        public int Depth { get; set; }
        public HashSet<ParameterExpression> ExtraParameters { get; } = new();
               
        internal AttributeMetadataCache AttributeMetadata { get; } = new();

        internal bool IsSubFunction()
        {
            return MaxDepth.HasValue || ExtraParameters.Count > 0;
        }

        public CompileContext(TypeAdapterConfig config)
        {
            Configs.Push(config);
        }

        internal TypeAdapterSettings GetMergedSettings(TypeTuple tuple, MapType mapType)
        {
            var arg = new PreCompileArgument
            {
                SourceType = tuple.Source,
                DestinationType = tuple.Destination,
                MapType = mapType,
                ExplicitMapping = MappingCache.GetRuleMap.ContainsKey(tuple),
                TypeMemberModelsCache = MappingCache.GetTypeMemberModelsCache

            };

            //auto add setting if there is attr setting
            var attrSettings = GetAttributeSettings(tuple, mapType).ToList();
            if (!arg.ExplicitMapping && attrSettings.Any(rule => rule.Priority(arg) == 100))
            {
                //GetSettings(tuple);
                arg.ExplicitMapping = true;
            }

            var result = new TypeAdapterSettings();
             
            var rules = MappingCache.GetRules.Reverse().ToList().Concat(attrSettings); // need refactoring
                
            var settings = from rule in rules
                               let priority = rule.Priority(arg)
                               where priority != null
                               orderby priority.Value descending
                               select rule.Settings;
                foreach (var setting in settings)
                {
                    result.Apply(setting);
                }
           

            //remove recursive include types
            if (mapType == MapType.MapToTarget)
                result.Includes.Remove(tuple);
            else
                result.Includes.RemoveAll(t => t.Source == tuple.Source);
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
                         select new TypeAdapterRule
                         {
                             Priority = arg => distance + 50,
                             Settings = CreateSettings(attr)
                         };
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
                         select new TypeAdapterRule
                         {
                             Priority = arg => distance + 50,
                             Settings = CreateSettings(attr)
                         };
            return rules1.Concat(rules2);
        }

        private TypeAdapterSettings CreateSettings(BaseAdaptAttribute attr)
        {
            var settings = new TypeAdapterSettings();
            var setter = new TypeAdapterSetter(settings, Config);
            setter.ApplyAdaptAttribute(attr);
            return settings;
        }

    }
}