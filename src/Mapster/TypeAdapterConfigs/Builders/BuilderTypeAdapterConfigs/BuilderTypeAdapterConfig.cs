using Mapster.Adapters;
using Mapster.Models;
using Mapster.TypeAdapterSetters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mapster.TypeAdapterConfigs.Builders
{
    public sealed class BuilderTypeAdapterConfig : ITypeAdapterConfigSettingsProducer
    {
        public bool RequireDestinationMemberSource { get => GetBoolSetting(nameof(RequireDestinationMemberSource)); set => SetBoolSetting(nameof(RequireDestinationMemberSource), value); }
        public bool RequireExplicitMapping { get => GetBoolSetting(nameof(RequireExplicitMapping)); set => SetBoolSetting(nameof(RequireExplicitMapping), value); }
        public bool RequireExplicitMappingPrimitive { get => GetBoolSetting(nameof(RequireExplicitMappingPrimitive)); set => SetBoolSetting(nameof(RequireExplicitMappingPrimitive), value); }
        public bool AllowImplicitDestinationInheritance { get => GetBoolSetting(nameof(AllowImplicitDestinationInheritance)); set => SetBoolSetting(nameof(AllowImplicitDestinationInheritance), value); }
        public bool AllowImplicitSourceInheritance { get => GetBoolSetting(nameof(AllowImplicitSourceInheritance)); set => SetBoolSetting(nameof(AllowImplicitSourceInheritance), value); }
        public bool SelfContainedCodeGeneration { get => GetBoolSetting(nameof(SelfContainedCodeGeneration)); set => SetBoolSetting(nameof(SelfContainedCodeGeneration), value); }
        public bool ActivateCustomConvertersSrcNullPropagation { get => GetBoolSetting(nameof(ActivateCustomConvertersSrcNullPropagation)); set => SetBoolSetting(nameof(ActivateCustomConvertersSrcNullPropagation), value); }
        public Func<LambdaExpression, Delegate> Compiler { get; set; } = MapsterGlobalConfig.DefaultCompiler;

        public DefaultOnlySetter Default 
        {
            get
            {
                if(DefaultRule is null)
                {
                    DefaultRule = CreateRuleHelper.CreateDefaultRule();
                    _default = new DefaultOnlySetter(DefaultRule.Settings, this);
                }

                return _default;
            } 
        }

        public BuilderTypeAdapterConfig()
        {
        }

        #region SettingsStorage

        private DefaultOnlySetter _default;
        public TypeAdapterRule DefaultRule;

        // AddOrUpdate existing settings. If none exist, create new ones.
        // ForType()
        public Dictionary<TypeTuple, TypeAdapterRule> UpdateRuleMaps { get; init; } = new();

        // Force update existing settings or create new ones.
        // NewConfig()
        public Dictionary<TypeTuple, TypeAdapterRule> NewRuleMaps { get; init; } = new(); 
        public List<TypeAdapterRule> InsertAdapterRules { get; init; } = new();
        public List<TypeAdapterRule> InsertCustomRules { get; init; } = new();

        public Dictionary<string, bool> BoolSettings { get; init; } = new();
        public bool GetBoolSetting(string name) { BoolSettings.TryGetValue(name, out var data); return data; }
        private void SetBoolSetting(string name, bool value) => BoolSettings[name] = value;

        #endregion SettingsStorage


        #region ProduceSettings
        public void AddAdapter(BaseAdapter adapter)
        {
            InsertAdapterRules.Add(adapter.CreateRule());
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
            UpdateRuleMaps[tuple] = rule;
            return new DestinationTypeOnlySetter(rule.Settings, this);
        }

        public TypeAdapterSetter<TDestination> ForDestinationType<TDestination>()
        {
            var tuple = new TypeTuple(typeof(void), typeof(TDestination));
            var rule = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            UpdateRuleMaps[tuple] = rule;
            return new TypeAdapterSetter<TDestination>(rule.Settings, this);
        }

        public TypeAdapterSetter ForType(Type sourceType, Type destinationType)
        {
            var tuple = new TypeTuple(sourceType, destinationType);
            var rule = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            UpdateRuleMaps[tuple] = rule;
            return new TypeAdapterSetter(rule.Settings,this);
        }

        public TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>()
        {
            var tuple = TypeTuple.Create<TSource,TDestination>();
            var rule = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            UpdateRuleMaps[tuple] = rule;
            return new TypeAdapterSetter<TSource,TDestination>(rule.Settings, this);
        }

        public TypeAdapterSetter NewConfig(Type sourceType, Type destinationType)
        {
            var tuple = new TypeTuple(sourceType, destinationType);
            var rule = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            NewRuleMaps[tuple] = rule;
            return new TypeAdapterSetter(rule.Settings, this);
        }

        public TypeAdapterSetter<TSource, TDestination> NewConfig<TSource, TDestination>()
        {
            var tuple = TypeTuple.Create<TSource,TDestination>();
            var rule = CreateRuleHelper.CreateRuleForTypeConfigBuilder(tuple, this);
            NewRuleMaps[tuple] = rule;
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

        public TypeAdapterSetter<TSource, TDestination> TwoWaysForType<TSource, TDestination>()
        {
            return NewConfig<TSource, TDestination>();
        }

        #endregion ProduceSettings

        #region UnSupprotedinBuilder


        public void CompileToSettingsSetters(Type sourceType, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public void CompileProjectionToSettingsSetters(Type sourceType, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public void CompileProjection()
        {
            throw new NotImplementedException();
        }

        public void Compile(bool failFast = true)
        {
            throw new NotImplementedException();
        }
        #endregion UnSupprotedinBuilder
    }
}
