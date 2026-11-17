using Mapster.Adapters;
using Mapster.Models;
using Mapster.TypeAdapterConfigs.Builders.Interfaces;
using Mapster.TypeAdapterSetters;
using System;
using System.Collections.Generic;
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



        public void AddAdapter(BaseAdapter adapter)
        {
            throw new NotImplementedException();
        }

        public void AddAdapterssRange(IEnumerable<BaseAdapter> adapters)
        {
            throw new NotImplementedException();
        }

        public void AddRule(TypeAdapterRule rule)
        {
            throw new NotImplementedException();
        }

        public void AddRulesRange(IEnumerable<TypeAdapterRule> rules)
        {
            throw new NotImplementedException();
        }

        public DestinationTypeOnlySetter ForDestinationType(Type destinationType)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter<TDestination> ForDestinationType<TDestination>()
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter ForType(Type sourceType, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter<TSource, TDestination> ForType<TSource, TDestination>()
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter NewConfig(Type sourceType, Type destinationType)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter<TSource, TDestination> NewConfig<TSource, TDestination>()
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter When(Func<PreCompileArgument, bool> canMap)
        {
            throw new NotImplementedException();
        }

        public TypeAdapterSetter When(Func<Type, Type, MapType, bool> canMap)
        {
            throw new NotImplementedException();
        }
    }
}
