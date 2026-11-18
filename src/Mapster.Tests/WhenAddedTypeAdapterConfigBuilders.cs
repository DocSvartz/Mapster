using Mapster.Models;
using Mapster.TypeAdapterConfigs.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Linq;

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
                .When(
                    canMap: (srcType, destType, mapType) => destType == typeof(ForBuilderDest), 
                    setup: s => { s.Settings.EnableNonPublicConstructors = true; })
                .Build();


            config.ShouldSatisfyAllConditions(() =>
            {
                config.Config.Default.ShouldNotBeNull();
                config.Config.DefaultRule.Settings.IgnoreNullValues.ShouldBe(true);

                config.Config.NewRuleMaps.Count.ShouldBe(1);
                config.Config.UpdateRuleMaps.Count.ShouldBe(0);
                config.Config.InsertAdapterRules.Count.ShouldBe(0);
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



                //resolvers[0].

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


        #endregion TestClases
    }
}
