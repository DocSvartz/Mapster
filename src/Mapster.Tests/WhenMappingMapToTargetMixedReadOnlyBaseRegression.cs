using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Mapster.Tests
{
    /// <summary>
    /// https://github.com/MapsterMapper/Mapster/issues/918
    /// </summary>
    [TestClass]
    public class WhenMappingMapToTargetMixedReadOnlyBaseRegression
    {
        [TestMethod]
        public void MapToTarget_SameType_WithMixedReadOnlyBaseProperties_UpdatesDestination()
        {
            var source = new Station918 { RowNo = 100, Location = "Source" };
            var destination = new Station918 { RowNo = 0, Location = "Destination" };

            source.Adapt(destination);

            destination.RowNo.ShouldBe(100);
            destination.Location.ShouldBe("Source");
        }

        public abstract class ValidationBase918
        {
            public bool HasErrors { get; }

            public string? ErrorMessage { get; }
        }

        public class Station918 : ValidationBase918
        {
            public int RowNo { get; set; }

            public string Location { get; set; } = string.Empty;
        }
    }
}
