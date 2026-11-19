using Mapster.Adapters;
using Mapster.Models;
using Mapster.TypeAdapterConfigs.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenAddedTypeAdapterConfigBuilders
    {
        [TestMethod]
        public void TypeAdapterConfigInitBuilderCreateConfigCorrect()
        {
            var config = TypeAdapterConfigInitBuilder.Create()
                .Default(cfg =>
                {
                    cfg.IgnoreNullValues(true);
                })
                .NewConfig<ForBuilderSrc, ForBuilderDest>(s =>
                {
                    s.Map(dest => dest.NameProxy, src => src.Name);
                    s.Map(dest => dest.AddressProxy, src => src.Address);
                })
                .AddAdapter<TestAdapter>()
                .When(
                    canMap: (srcType, destType, mapType) => destType == typeof(ForBuilderDest), 
                    setup: s => { s.Settings.EnableNonPublicConstructors = true; })
                .TwoWays<TwoWaysSrc, TwoWaysDest>(cfg =>
                {
                    cfg.ShallowCopyForSameType(true);
                })
                .Build();


            config.ShouldSatisfyAllConditions(() =>
            {
                config.Config.Default.ShouldNotBeNull();
                config.Config.DefaultRule.Settings.IgnoreNullValues.ShouldBe(true);

                config.Config.NewRuleMaps.Count.ShouldBe(3);
                config.Config.UpdateRuleMaps.Count.ShouldBe(0);
                config.Config.InsertAdapterRules.Count.ShouldBe(1);
                config.Config.InsertCustomRules.Count.ShouldBe(1);

                var key = TypeTuple.Create<ForBuilderSrc, ForBuilderDest>();
                var resolvers = config.Config.NewRuleMaps[key].Settings.Resolvers;
                resolvers.Count.ShouldBe(2);
                var resolverForName = resolvers.First();
                var resolverForAddress = resolvers.Last();

                resolverForName.SourceMemberName.ShouldBe(nameof(ForBuilderSrc.Name));
                resolverForName.DestinationMemberName.ShouldBe(nameof(ForBuilderDest.NameProxy));
                resolverForAddress.SourceMemberName.ShouldBe(nameof(ForBuilderSrc.Address));
                resolverForAddress.DestinationMemberName.ShouldBe(nameof(ForBuilderDest.AddressProxy));

                //WhenRules check
                var WhenRule1 = config.Config.InsertCustomRules.First();
                WhenRule1.Settings.EnableNonPublicConstructors.ShouldBe(true);

                //AddAdapterCheck
                var adapterRule = config.Config.InsertAdapterRules.First();
                adapterRule.Settings.DestinationType.ShouldBe(typeof(TestAdapter));

                //TwoWays
                config.Config.NewRuleMaps.ContainsKey(TypeTuple.Create<TwoWaysSrc, TwoWaysDest>()).ShouldBeTrue();
                config.Config.NewRuleMaps.ContainsKey(TypeTuple.Create<TwoWaysDest, TwoWaysSrc>()).ShouldBeTrue();
                
                var DirectRule = config.Config.NewRuleMaps[TypeTuple.Create<TwoWaysSrc, TwoWaysDest>()];
                var ReverseWayRule = config.Config.NewRuleMaps[TypeTuple.Create<TwoWaysSrc, TwoWaysDest>()];

                DirectRule.Settings.ShallowCopyForSameType.ShouldBe(true);
                ReverseWayRule.Settings.ShallowCopyForSameType.ShouldBe(true);
            });
        }

        #region TestClases

        public class ForBuilderSrc
        {
            public string Name { get; set; }

            public string Address { get; set; }
        }

        public class ForBuilderDest
        {
            public string NameProxy { get; set; }

            public string AddressProxy { get; set; }
        }

        public class TwoWaysSrc
        {
            public string TwoWaySrc { get; set; }

        }

        public class TwoWaysDest
        {
            public string TwoWayDest { get; set; }

        }


        internal class TestAdapter : BaseAdapter
        {
            protected override bool CanMap(PreCompileArgument arg)
            {
                throw new NotImplementedException();
            }

            protected override Expression CreateBlockExpression(Expression source, Expression destination, CompileArgument arg)
            {
                throw new NotImplementedException();
            }

            protected override Expression CreateInlineExpression(Expression source, CompileArgument arg)
            {
                throw new NotImplementedException();
            }
        }




        #endregion TestClases
    }
}
