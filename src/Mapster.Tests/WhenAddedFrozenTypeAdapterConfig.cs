using Mapster.TypeAdapterConfigs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using static Mapster.Tests.WhenAddedTypeAdapterConfigBuilders;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenAddedFrozenTypeAdapterConfig
    {
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
    }
}
