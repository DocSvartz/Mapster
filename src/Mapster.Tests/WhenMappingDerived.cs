using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenMappingDerived
    {
        [TestCleanup]
        public void TestCleanup()
        {
            TypeAdapterConfig.GlobalSettings.Clear();
        }

        [TestMethod]
        public void WhenCompilingConfigDerivedWithoutMembers()
        {
            //Arrange
            var config = TypeAdapterConfig<Entity, DerivedDto>.NewConfig()
                                                           .ConstructUsing(entity => new DerivedDto(entity.Id))
                                                           .Ignore(domain => domain.Id)
                                                           ;

            //Act && Assert
            Should.NotThrow(() => config.Compile());
        }

        [TestMethod]
        public void WhenMappingDerivedWithoutMembers()
        {
            //Arrange
            var inputEntity = new Entity {Id = 2L};

            var config = TypeAdapterConfig<Entity, DerivedDto>.NewConfig()
                                                           .ConstructUsing(entity => new DerivedDto(entity.Id))
                                                           .Ignore(domain => domain.Id)
                                                           ;
            config.Compile();
            //Act
            var result = TypeAdapter.Adapt<Entity, DerivedDto>(inputEntity);

            //Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(inputEntity.Id, result.Id);
        }

        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/794
        /// </summary>
        [TestMethod]
        public void WhenMapToTargetDerivedWithNullRegression()
        {
            var config = new TypeAdapterConfig();

            config
               .NewConfig<ContainerDTO794, Container794>()
               .Map(dest => dest.Nested, src => src.NestedDTO)
               .IgnoreNonMapped(true)
               .IgnoreNullValues(true);
            config
                .NewConfig<BaseDTO794, Base794>()
                .Map(dest => dest.SomeBaseProperty, src => src.SomeBasePropertyDTO)
                .Include<DerivedDTO794, Base794>()
                .IgnoreNonMapped(true)
                .IgnoreNullValues(true);

            config
                .NewConfig<DerivedDTO794, Derived794>()
                .Map(dest => dest.SomeDerivedProperty, src => src.SomeDerivedPropertyDTO)
                .IgnoreNonMapped(true)
                .IgnoreNullValues(true);
            config
                .NewConfig<DerivedDTO794, Base794>()
                .MapWith(src => src.Adapt<Derived794E>());


            var container = new Container794();
            var containerDTO = new ContainerDTO794();

            container.Nested = null;
            containerDTO.NestedDTO = new DerivedDTO794();

            containerDTO.Adapt<ContainerDTO794, Container794>(container, config);

            (container.Nested is Derived794E).ShouldBeTrue(); // is not Base794 type, MapWith is working when Polymorphic mapping to null
        }

        internal class Derived794E : Derived794
        {

        }

        internal class Base794
        {
            public string SomeBaseProperty { get; set; }
        }

        internal class BaseDTO794
        {
            public string SomeBasePropertyDTO { get; set; }
        }

        internal class Derived794 : Base794
        {
            public string SomeDerivedProperty { get; set; }
        }

        internal class DerivedDTO794 : BaseDTO794
        {
            public string SomeDerivedPropertyDTO { get; set; }
        }

        internal class Container794
        {
            public Base794 Nested { get; set; }
        }

        internal class ContainerDTO794
        {
            public BaseDTO794 NestedDTO { get; set; }
        }

        internal class BaseDto
        {
            public long Id { get; set; }

            protected BaseDto(long id)
            {
                Id = id;
            }
        }

        internal class Entity
        {
            public long Id { get; set; }
        }

        internal class DerivedDto : BaseDto
        {
            public DerivedDto(long id) : base(id) { }
        }
    }
}
