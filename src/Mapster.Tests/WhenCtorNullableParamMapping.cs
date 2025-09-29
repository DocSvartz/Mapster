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
            TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

            TypeAdapterConfig<AbstractDtoTestClass, AbstractDomainTestClass>
                .NewConfig()
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

            TypeAdapterConfig<DtoTestClass, DomainTestClass>.NewConfig()
               .Map(x => x.AbstractType, y => y.AbstractType, z => z.AbstractType != null);

            var domain = dto.Adapt<DomainTestClass>();

            domain.AbstractType.ShouldNotBe(null);
            domain.AbstractType.ShouldBeOfType<DerivedDomainTestClass>();

            var domainDerived = (DerivedDomainTestClass)domain.AbstractType;
            domainDerived.DerivedProperty.ShouldBe(dtoDerived.DerivedProperty);
            domainDerived.AbstractProperty.ShouldBe(dtoDerived.AbstractProperty);

        }

        [TestMethod]
        public void Dto_To_Domain_AbstractClassNull_MapsCorrectly()
        {
            TypeAdapterConfig.GlobalSettings.Default.MapToConstructor(true);

            TypeAdapterConfig<AbstractDtoTestClass, AbstractDomainTestClass>
                .NewConfig()
                .Include<DerivedDtoTestClass, DerivedDomainTestClass>();

            var dto = new DtoTestClass
            {
                AbstractType = null
            };

            var domain = dto.Adapt<DomainTestClass>();

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
