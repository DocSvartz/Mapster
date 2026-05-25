using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Mapster.Tests
{
    /// <summary>
    /// https://github.com/MapsterMapper/Mapster/issues/947
    /// </summary>
    [TestClass]
    public class WhenImplicitInheritanceMapWithDerivedDestination
    {
        [TestCleanup]
        public void Cleanup()
        {
            TypeAdapterConfig.GlobalSettings.Clear();
            TypeAdapterConfig.GlobalSettings.AllowImplicitDestinationInheritance = false;
        }

        [TestMethod]
        public void Inherited_MapWith_On_Base_Destination_Casts_To_Derived_Destination()
        {
            var config = new TypeAdapterConfig();
            config.AllowImplicitDestinationInheritance = true;
            config.NewConfig<AnimalDto947, Animal947>()
                .MapWith(src => src.Type == "Bird"
                    ? (Animal947)new Bird947 { AnimalValue = src.AnimalValueDto }
                    : new Dog947 { AnimalValue = src.AnimalValueDto });

            var source = new AnimalDto947 { AnimalValueDto = "Hello", Type = "Dog" };

            var dog = source.Adapt<Dog947>(config);

            dog.ShouldBeOfType<Dog947>();
            dog.AnimalValue.ShouldBe("Hello");
        }

        [TestMethod]
        public void Inherited_MapWith_Works_For_Explicit_Source_Destination_Pair()
        {
            var config = new TypeAdapterConfig();
            config.AllowImplicitDestinationInheritance = true;
            config.NewConfig<AnimalDto947, Animal947>()
                .MapWith(src => src.Type == "Bird"
                    ? (Animal947)new Bird947 { AnimalValue = src.AnimalValueDto }
                    : new Dog947 { AnimalValue = src.AnimalValueDto });

            var source = new AnimalDto947 { AnimalValueDto = "Hello", Type = "Dog" };

            var dog = source.Adapt<AnimalDto947, Dog947>(config);

            dog.ShouldBeOfType<Dog947>();
            dog.AnimalValue.ShouldBe("Hello");
        }

        public abstract class Animal947
        {
            public string AnimalValue { get; set; } = null!;
        }

        public class Dog947 : Animal947
        {
        }

        public class Bird947 : Animal947
        {
        }

        public class AnimalDto947
        {
            public string AnimalValueDto { get; set; } = null!;

            public string Type { get; set; } = null!;
        }
    }
}
