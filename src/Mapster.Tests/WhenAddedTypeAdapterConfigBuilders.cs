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

                //NewConfig check
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

        [TestMethod]
        public void TypeAdapterConfigPatchBuilderCreateConfigCorrect()
        {
            var config = TypeAdapterConfigPatchBuilder.Create()
                .Default(cfg =>
                {
                    cfg.IgnoreNullValues(true);
                })
                //Available only in Patch Builder
                .ForType<ForBuilderSrc, ForBuilderDest>(s => 
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

                config.Config.NewRuleMaps.Count.ShouldBe(2);
                config.Config.UpdateRuleMaps.Count.ShouldBe(1);
                config.Config.InsertAdapterRules.Count.ShouldBe(1);
                config.Config.InsertCustomRules.Count.ShouldBe(1);

                //ForType check
                var key = TypeTuple.Create<ForBuilderSrc, ForBuilderDest>();
                var resolvers = config.Config.UpdateRuleMaps[key].Settings.Resolvers;
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

        [TestMethod]
        public void TypeAdapterConfigBuilder_ThrowException_WhenAdded_Dublicate_Settings()
        {
            var s = new object();

            s.ShouldSatisfyAllConditions(() =>
            {
                Should.Throw<InvalidOperationException>(() => {
                    TypeAdapterConfigInitBuilder.Create()
                    .Default(cfg =>{})
                    .NewConfig<ForBuilderSrc, ForBuilderDest>(s =>{})
                    .Default(cfg =>{}) // dublicate
                    .Build();
                });

                Should.Throw<InvalidOperationException>(() => {
                    TypeAdapterConfigInitBuilder.Create()
                    .NewConfig<ForBuilderSrc, ForBuilderDest>(s => { })
                    .Default(cfg => { })
                    .NewConfig<ForBuilderSrc, ForBuilderDest>(s => { }) // dublicate
                    .Build();
                });

                Should.Throw<InvalidOperationException>(() => {
                    TypeAdapterConfigInitBuilder.Create()
                    .NewConfig<ForBuilderSrc, ForBuilderDest>(s => { })
                    .TwoWays<ForBuilderSrc, ForBuilderDest> (s => { }) // dublicate
                    .Build();
                });

                Should.Throw<InvalidOperationException>(() => {
                    TypeAdapterConfigPatchBuilder.Create()
                    .NewConfig<ForBuilderSrc, ForBuilderDest>(s => { })
                    .ForType<ForBuilderSrc, ForBuilderDest>(s => { }) // dublicate
                    .Build();
                });

                Should.Throw<InvalidOperationException>(() => {
                    TypeAdapterConfigPatchBuilder.Create()
                    .AllowImplicitDestinationInheritance(true)
                    .Default(cfg => { })
                    .AllowImplicitDestinationInheritance(false) // dublicate
                    .Build();
                });

                Should.Throw<InvalidOperationException>(() => {
                    TypeAdapterConfigPatchBuilder.Create()
                    .AddAdapter<TestAdapter>()
                    .Default(cfg => { })
                    .AddAdapter(new TestAdapter()) // dublicate
                    .Build();
                });

            });
        }
        
        
        [TestMethod]
        public void TestPatchTypeAdapterConfig()
        {
            var config = new TypeAdapterConfig();
            var src = new ForBuilderSrc { Address = "Default street", Name = "John" };

            config.Patch
                .NewConfig<ForBuilderSrc, ForBuilderDest>(cfg =>
                {
                    cfg.Map(dest => dest.NameProxy, src => src.Address);
                    cfg.Map(dest => dest.AddressProxy, src => src.Name);
                });

            MapsterGlobalConfig.GlobalSettings
                .Patch
                .NewConfig<ForBuilderSrc, ForBuilderDest>(cfg =>
                {
                    cfg.Map(dest => dest.NameProxy, src => src.Name);
                    cfg.Map(dest => dest.AddressProxy, src => src.Address);
                });
            

            new object().ShouldSatisfyAllConditions(() =>
            {
                config.TryPatchApply(out var Snapshot).ShouldBeTrue();
                MapsterGlobalConfig.GlobalSettings.TryPatchApply(out _);

                var result = src.Adapt<ForBuilderSrc, ForBuilderDest>();
                var snapResult = src.Adapt<ForBuilderDest>(MapsterGlobalConfig.GlobalSettings.GetSnapshot);

                var localResult = src.Adapt<ForBuilderSrc, ForBuilderDest>(config);
                var snapLocalResult = src.Adapt<ForBuilderDest>(config.GetSnapshot);

                result.AddressProxy.ShouldBe(src.Address);
                snapResult.AddressProxy.ShouldBe(src.Address);
                result.NameProxy.ShouldBe(src.Name);
                snapResult.NameProxy.ShouldBe(src.Name);

                localResult.AddressProxy.ShouldBe(src.Name);
                snapLocalResult.AddressProxy.ShouldBe(src.Name);
                localResult.NameProxy.ShouldBe(src.Address);
                snapLocalResult.NameProxy.ShouldBe(src.Address);

            });
        }

        [TestMethod]
        public void TestPatchApplyTypeAdapterConfig()
        {
            var config = new TypeAdapterConfig();
            var src = new ForBuilderSrc { Address = "Default street", Name = "John" };


            config.PatchApply(out var Snapshot, patch =>
            {
                patch.NewConfig<ForBuilderSrc, ForBuilderDest>(cfg =>
                 {
                     cfg.Map(dest => dest.NameProxy, src => src.Address);
                     cfg.Map(dest => dest.AddressProxy, src => src.Name);
                 });
            });

            MapsterGlobalConfig.GlobalSettings.PatchApply(out var GlobalSnapShot, patch =>
            {
                patch.NewConfig<ForBuilderSrc, ForBuilderDest>(cfg =>
                {
                    cfg.Map(dest => dest.NameProxy, src => src.Name);
                    cfg.Map(dest => dest.AddressProxy, src => src.Address);
                });
            });


            new object().ShouldSatisfyAllConditions(() =>
            {
                var result = src.Adapt<ForBuilderSrc, ForBuilderDest>();
                var snapResult = src.Adapt<ForBuilderDest>(MapsterGlobalConfig.GlobalSettings.GetSnapshot);

                var localResult = src.Adapt<ForBuilderSrc, ForBuilderDest>(config);
                var snapLocalResult = src.Adapt<ForBuilderDest>(config.GetSnapshot);

                result.AddressProxy.ShouldBe(src.Address);
                snapResult.AddressProxy.ShouldBe(src.Address);
                result.NameProxy.ShouldBe(src.Name);
                snapResult.NameProxy.ShouldBe(src.Name);

                localResult.AddressProxy.ShouldBe(src.Name);
                snapLocalResult.AddressProxy.ShouldBe(src.Name);
                localResult.NameProxy.ShouldBe(src.Address);
                snapLocalResult.NameProxy.ShouldBe(src.Address);
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
