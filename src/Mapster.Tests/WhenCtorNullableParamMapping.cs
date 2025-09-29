using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

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
