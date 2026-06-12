using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenCtorNullableParamMapping
    {
        [TestMethod]
        public void Dto_To_Domain_MapsCorrectly()
        {
            var config = new TypeAdapterConfig();

            config.Default.MapToConstructor(true);
            config
                .NewConfig<AbstractDtoTestClass, AbstractDomainTestClass>()
                .Include<DerivedDtoTestClass, DerivedDomainTestClass>();


            var dtoDerived = new DerivedDtoTestClass
            {
                DerivedProperty = "DerivedValue",
                AbstractProperty = "AbstractValue"
            };

            var dto = new DtoTestClass
            {
                AbstractType = dtoDerived
            };

            var domain = dto.Adapt<DomainTestClass>(config);

            domain.AbstractType.ShouldNotBe(null);
            domain.AbstractType.ShouldBeOfType<DerivedDomainTestClass>();

            var domainDerived = (DerivedDomainTestClass)domain.AbstractType;
            domainDerived.DerivedProperty.ShouldBe(dtoDerived.DerivedProperty);
            domainDerived.AbstractProperty.ShouldBe(dtoDerived.AbstractProperty);

        }

        [TestMethod]
        public void Dto_To_Domain_AbstractClassNull_MapsCorrectly()
        {
            var config = new TypeAdapterConfig();

            config.Default.MapToConstructor(true);
            config
                .NewConfig<AbstractDtoTestClass, AbstractDomainTestClass>()
                .Include<DerivedDtoTestClass, DerivedDomainTestClass>();

            var dto = new DtoTestClass
            {
                AbstractType = null
            };

            var domain = dto.Adapt<DomainTestClass>(config);

            domain.AbstractType.ShouldBeNull();
        }


        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/943
        /// </summary>
        [TestMethod]
        public void NullableCtorPropagationCurrentWorkWithDestinationTransform()
        {
            var config = new TypeAdapterConfig();

            config.Default
                .AddDestinationTransform(DestinationTransform.EmptyCollectionIfNull);

            // Arrange
            var fooDto = new FooDto943();

            // Act
            var foo = fooDto.Adapt<Foo943>(config);

            // Assert
            foo.Strings.ShouldNotBeNull();
        }


        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/954
        /// </summary>
        [TestMethod]
        public void MappingValueTypeParametrUsingDefaultValueCorrect()
        {
            // Arrange
            var src = new DateTimeFoo954(DateTime.Today);

            // Assert
            Should.NotThrow(() =>
            {
                var foo = src.Adapt<DateTimeFooDto954>();

                foo.Timestamp.ShouldBe(src.Timestamp);
            });
        }



        #region Immutable classes with private setters, map via ctors
        private abstract class AbstractDomainTestClass
        {
            public string AbstractProperty { get; private set; }

            protected AbstractDomainTestClass(string abstractProperty)
            {
                AbstractProperty = abstractProperty;
            }
        }

        private class DerivedDomainTestClass : AbstractDomainTestClass
        {
            public string DerivedProperty { get; private set; }

            /// <inheritdoc />
            public DerivedDomainTestClass(string abstractProperty, string derivedProperty)
                : base(abstractProperty)
            {
                DerivedProperty = derivedProperty;
            }
        }

        private class DomainTestClass
        {
            public AbstractDomainTestClass? AbstractType { get; private set; }

            public DomainTestClass(
                AbstractDomainTestClass? abstractType)
            {
                AbstractType = abstractType;
            }
        }
        #endregion

        #region DTO classes

        public class DateTimeFooDto954
        {
            public DateTime Timestamp { get; set; }

            public DateTimeFooDto954(DateTime timestamp = default(DateTime))
            {
                this.Timestamp = timestamp;
            }
        }

        record DateTimeFoo954(DateTime Timestamp);

        class FooDto943
        {
            public string[] Strings { get; set; }
        }

        record Foo943(List<string> Strings);
        private abstract class AbstractDtoTestClass
        {
            public string AbstractProperty { get; set; }
        }

        private class DerivedDtoTestClass : AbstractDtoTestClass
        {
            public string DerivedProperty { get; set; }
        }

        private class DtoTestClass
        {
            public AbstractDtoTestClass? AbstractType { get; set; }
        }
        #endregion
    }
}
