using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Mapster.Tests
{

    public record SourceDto(string Name, int Age);
    public record DestinationDto(long Id, string Name, int Age);

    [TestClass]
    public class WhenUseTempAdapterConfig
    {
        [TestMethod]
        public void Adapt_TemporaryConfig_ShouldMapInitOnlyProperties()
        {
            // Arrange
            var source = new SourceDto("Alice", 30);
            long id = 42;

            // Act
            var result = source.Adapt<DestinationDto>(cfg =>
            {
                cfg.NewConfig<SourceDto, DestinationDto>()
                   .Map(dest => dest.Id, src => id);
            });

            // Assert
            result.Name.ShouldBe("Alice");
            result.Age.ShouldBe(30);
            result.Id.ShouldBe(42);
        }

        [TestMethod]
        public void Adapt_WithSetter_ShouldMapInitOnlyProperties()
        {
            // Arrange
            var source = new SourceDto("Bob", 25);
            long id = 99;

            // Act
            var result = source.Adapt<SourceDto, DestinationDto>(setter =>
            {
                setter.Map(dest => dest.Id, src => id);
            });

            // Assert
            result.Name.ShouldBe("Bob");
            result.Age.ShouldBe(25);
            result.Id.ShouldBe(99);
        }

        [TestMethod]
        public void Adapt_TemporaryConfig_ShouldNotModifyGlobalSettings()
        {
            // Arrange
            var source = new SourceDto("Charlie", 40);
            long id = 123;

            var globalMap = MapsterGlobalConfig.GlobalSettings.GetSnapshot.GetMapFunction<SourceDto, DestinationDto>();

            // Act
            var result = source.Adapt<SourceDto, DestinationDto>(setter =>
            {
                setter.Map(dest => dest.Id, src => id);
            });

            // Assert
            var original = globalMap(source); // mapping via GlobalSettings
            original.Id.ShouldBe(default(long)); // GlobalSettings unaffected
            original.Name.ShouldBe("Charlie");
            original.Age.ShouldBe(40);
        }

    }
}
