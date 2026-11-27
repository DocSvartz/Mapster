using Mapster.Models;
using Mapster.TypeAdapterConfigs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using static Mapster.Tests.WhenAddedTypeAdapterConfigBuilders;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenAddedFrozenTypeAdapterConfig
    {
        [TestCleanup]
        public void CleanUp()
        {
            var config = typeof(FrozenGlobalConfig).GetField(nameof(FrozenGlobalConfig.GlobalSettings));
            config!.SetValue(null, new FrozenTypeAdapterConfig());

            var snapshot = typeof(FrozenGlobalConfig).GetField(nameof(FrozenGlobalConfig.Snapshot));
            snapshot!.SetValue(null, null);
        }


        [TestMethod]
        public void GlobalFrozenConfigInit()
        {
            FrozenGlobalConfig.Init(out var snapshot, init =>
            {
                init.Compiler("add compiler", MapsterGlobalConfig.GlobalSettings.Compiler);
                init.NewConfig<ForBuilderSrc, ForBuilderDest>(s =>
                 {
                     s.Map(dest => dest.NameProxy, src => src.Name);
                     s.Map(dest => dest.AddressProxy, src => src.Address);
                 });
            });

            FrozenGlobalConfig.Snapshot.ShouldNotBeNull();

            object.ReferenceEquals(FrozenGlobalConfig.Snapshot, snapshot).ShouldBeTrue();
        }

        [TestMethod]
        public void GlobalFrozenConfig_ScanBuilderCreateConfig()
        {
            var applyBuilders = new List<IRegisterBuilder>()
            { new BuldConfigForBuilderSrc(), new BuldConfigForBuilderDest() };

            FrozenGlobalConfig.Apply(applyBuilders);

            FrozenGlobalConfig.Snapshot.ShouldSatisfyAllConditions(() =>
            {
                var snap = FrozenGlobalConfig.Snapshot;

                snap.RuleMap.Count.ShouldBe(2);

                snap.RuleMap.ContainsKey(TypeTuple.Create<ForBuilderSrc, ForBuilderDest>()).ShouldBeTrue();
                snap.RuleMap.ContainsKey(TypeTuple.Create<ForBuilderDest, ForBuilderSrc>()).ShouldBeTrue();
            });

            var snap = FrozenGlobalConfig.Snapshot;
        }
    }
}
