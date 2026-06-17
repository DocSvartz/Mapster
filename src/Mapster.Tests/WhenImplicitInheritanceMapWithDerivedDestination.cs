using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using static Mapster.Tests.DynamicTypeGeneratorTests;

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
            var sourceInsaider = new AnimalDtoInsaider947() { Animal = source };

            var dog = source.Adapt<Dog947>(config);
            var dogInsaider = sourceInsaider.Adapt<DogInsaider947>(config);
                        
            dog.ShouldBeOfType<Dog947>();
            dog.AnimalValue.ShouldBe("Hello");

            dogInsaider.Animal.ShouldBeOfType<Dog947>();
            dogInsaider.Animal.AnimalValue.ShouldBe("Hello");
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


        [TestMethod]
        public void Inherited_MapWith_On_Base_Destination_ReturnDefault_When_In_Runtime_ResultType_IsNot_Achievable()
        {
            var config = new TypeAdapterConfig();
            config.AllowImplicitDestinationInheritance = true;
            config.NewConfig<AnimalDto947, Animal947>()
                .MapWith(src => src.Type == "Bird"
                    ? (Animal947)new Bird947 { AnimalValue = src.AnimalValueDto }
                    : new Dog947 { AnimalValue = src.AnimalValueDto });

            var source = new AnimalDto947 { AnimalValueDto = "Hello", Type = "Bird" };

            var dog = source.Adapt<Dog947>(config);

            dog.ShouldBeNull();
            dog.ShouldBe(default);
        }


        [TestMethod]
        public void Inherited_MapWith_On_Base_Destination_Casts_To_Derived_Destination_UsingInterface()
        {
            var config = new TypeAdapterConfig();
            config.AllowImplicitDestinationInheritance = true;
            config.NewConfig<AnimalDto947, IAnimal>()
                .MapWith(src => src.Type == "Bird"
                    ? new ValueTypeBird947 { AnimalValue = src.AnimalValueDto }
                    : new ValueTypeDog947 { AnimalValue = src.AnimalValueDto });

            var dog = new AnimalDto947 { AnimalValueDto = "Hello", Type = "Dog" }.Adapt<ValueTypeDog947>(config);
            var defaultdata = new AnimalDto947 { AnimalValueDto = "Hello", Type = "Bird" }.Adapt<ValueTypeDog947>(config);

            dog.ShouldBeOfType<ValueTypeDog947>();
            dog.AnimalValue.ShouldBe("Hello");

            defaultdata.ShouldBeOfType<ValueTypeDog947>();
            defaultdata.AnimalValue.ShouldBe(default);
        }

        [TestMethod]
        public void Inherited_MapWith_On_Base_Destination_Casts_To_Derived_Destination_NullableValueType()
        {
            var config = new TypeAdapterConfig();
            config.AllowImplicitDestinationInheritance = true;
            config.NewConfig<AnimalDto947, IAnimal>()
                .MapWith(src => src.Type == "Bird"
                    ? new ValueTypeBird947 { AnimalValue = src.AnimalValueDto }
                    : new ValueTypeDog947 { AnimalValue = src.AnimalValueDto });

            var validSrc = new AnimalDto947 { AnimalValueDto = "Hello", Type = "Dog" };
            var invalidSrc = new AnimalDto947 { AnimalValueDto = "Tweet", Type = "Bird" }; ;

            var dog = validSrc.Adapt<ValueTypeDog947?>(config);
            var Nulldata = invalidSrc.Adapt<ValueTypeDog947?>(config);
            var InsaiderNullableDog = new AnimalDtoInsaider947() { Animal = validSrc}.Adapt<DogValueTypeNullableInsaider947>(config);
            var NullInsaiderNullableDog = new AnimalDtoInsaider947() { Animal = invalidSrc }.Adapt<DogValueTypeNullableInsaider947>(config);

            dog.ShouldNotBeNull();
            dog?.AnimalValue.ShouldBe("Hello");
            InsaiderNullableDog.Animal.ShouldNotBeNull();
            InsaiderNullableDog.Animal?.AnimalValue.ShouldBe("Hello");


            Nulldata.ShouldBeNull();
            NullInsaiderNullableDog.Animal.ShouldBeNull();
        }


        #region TestClases

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

        public class AnimalDtoInsaider947
        {
            public AnimalDto947 Animal { get; set; }
        }

        public class DogInsaider947
        {
            public Dog947 Animal { get; set; }
        }

        public interface IAnimal
        {
            public string AnimalValue { get; set; }
        }

        public struct ValueTypeBird947 : IAnimal
        {
            public string AnimalValue { get; set; }
        }

        public struct ValueTypeDog947 : IAnimal
        {
            public string AnimalValue { get; set; }

        }

        public class DogValueTypeNullableInsaider947
        {
            public ValueTypeDog947? Animal { get; set; }
        }

        #endregion TestClases
    }
}
