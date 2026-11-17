using Mapster.Adapters;
using Mapster.Models;
using Mapster.TypeAdapterConfigs.Builders.Interfaces;
using Mapster.TypeAdapterSetters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mapster.TypeAdapterConfigs.Builders.BuilderTypeAdapterConfigs
{
    internal class PatchBuilderTypeAdapterConfig : ITypeAdapterConfigSettingsCreator
    {
        public bool ActivateCustomConvertersSrcNullPropagation {  get; set; }
        public bool AllowImplicitDestinationInheritance { get; set; }
        public bool AllowImplicitSourceInheritance { get; set; }
        public bool RequireDestinationMemberSource { get; set; }
        public bool RequireExplicitMapping { get; set; }
        public bool RequireExplicitMappingPrimitive { get; set; }
        public bool SelfContainedCodeGeneration { get; set; }
        public Func<LambdaExpression, Delegate> Compiler { get; set; }

        public DefaultOnlySetter Default => throw new NotImplementedException();

        #region SettingsStorage

        // Try update existing settings. If none exist, create new ones.
        // ForType()
        public Dictionary<TypeTuple, TypeAdapterRule> UpdateRuleMaps { get; init; } = new();

        // Force update existing settings or create new ones.
        // NewConfig()
        public Dictionary<TypeTuple, TypeAdapterRule> NewRuleMaps { get; init; } = new(); 
        public List<TypeAdapterRule> InsertAdapterRules { get; init; } = new();
        public List<TypeAdapterRule> InsertCustomRules { get; init; } = new();

        #endregion SettingsStorage


        #region ProduceSettings
        public void AddAdapter(BaseAdapter adapter)
        {
            InsertCustomRules.Add(adapter.CreateRule());
        }

        public void AddAdapterssRange(IEnumerable<BaseAdapter> adapters)
        {
            InsertAdapterRules.AddRange(adapters.Select(x => x.CreateRule()));
        }

        public void AddRule(TypeAdapterRule rule)
        {
            InsertCustomRules.Add(rule);
        }

        public void AddRulesRange(IEnumerable<TypeAdapterRule> rules)
        {
            InsertCustomRules.AddRange(rules);
        }

        public DestinationTypeOnlySetter ForDestinationType(Type destinationType)
        {
            var tuple = new TypeTuple(typeof(void), destinationType);
            var rule = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            UpdateRuleMaps[tuple] = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            return new DestinationTypeOnlySetter(rule.Settings, this);
        }

        public TypeAdapterSetter<TDestination> ForDestinationType<TDestination>()
        {
            var tuple = new TypeTuple(typeof(void), typeof(TDestination));
            var rule = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            UpdateRuleMaps[tuple] = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            return new TypeAdapterSetter<TDestination>(rule.Settings, this);
        }

        public TypeAdapterSetter ForType(Type sourceType, Type destinationType)
        {
            var tuple = new TypeTuple(sourceType, destinationType);
            var rule = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            UpdateRuleMaps[tuple] = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            return new TypeAdapterSetter(rule.Settings,this);
        }

        public TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>()
        {
            var tuple = TypeTuple.Create<TSource,TDestination>();
            var rule = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            UpdateRuleMaps[tuple] = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            return new TypeAdapterSetter<TSource,TDestination>(rule.Settings, this);
        }

        public TypeAdapterSetter NewConfig(Type sourceType, Type destinationType)
        {
            var tuple = new TypeTuple(sourceType, destinationType);
            var rule = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            NewRuleMaps[tuple] = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            return new TypeAdapterSetter(rule.Settings, this);
        }

        public TypeAdapterSetter<TSource, TDestination> NewConfig<TSource, TDestination>()
        {
            var tuple = TypeTuple.Create<TSource,TDestination>();
            var rule = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            NewRuleMaps[tuple] = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            return new TypeAdapterSetter<TSource,TDestination>(rule.Settings, this);
        }

        public TypeAdapterSetter When(Func<PreCompileArgument, bool> canMap)
        {
            var rule = CreateRuleHelper.CreateWhenRule(canMap);
            InsertCustomRules.Add(rule);
            return new TypeAdapterSetter(rule.Settings, this);
        }

        public TypeAdapterSetter When(Func<Type, Type, MapType, bool> canMap)
        {
            var rule = CreateRuleHelper.CreateWhenRule(canMap);
            InsertCustomRules.Add(rule);
            return new TypeAdapterSetter(rule.Settings, this);
        }

        #endregion ProduceSettings
    }
}
