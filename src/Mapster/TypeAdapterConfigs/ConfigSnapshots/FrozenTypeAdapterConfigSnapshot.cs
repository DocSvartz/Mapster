using Mapster.Models;
using Mapster.TypeAdapterConfigs.Builders;
using Mapster.TypeAdapterConfigs.ConfigSnapshots;
using System;
using System.Collections.Immutable;
using System.Linq.Expressions;

namespace Mapster
{
    public class FrozenTypeAdapterConfigSnapshot : BaseNotStrictConfigSnapshot
    {
        internal FrozenTypeAdapterConfigSnapshot(BuilderTypeAdapterConfig init) 
        {
            TemplateRules = MapsterGlobalConfig.RulesTemplate;
            Compiler = init.Compiler;
            BoolSettings = init.BoolSettings.ToImmutableDictionary();
            RuleMap = init.NewRuleMaps;
            DefaultSettingsRule = init.DefaultRule;
            AdapterRules = init.InsertAdapterRules;
            CustomSettingsRules = init.InsertCustomRules;
        }
        internal FrozenTypeAdapterConfigSnapshot(FrozenTypeAdapterConfigSnapshot old, BuilderTypeAdapterConfig patch) 
        {

        }

        protected override Expression CreateSelfExpression()
        {
            return Expression.Constant(this);
        }

        protected override void InvalidateGenericCacheMapFunc(TypeTuple tuple)
        {
            //
        }

        protected override bool TryGetInvokeGenericCacheMapFunc(TypeTuple tuple, out Expression? invoker)
        {
            invoker = null;
            return false;
        }
    }
}
