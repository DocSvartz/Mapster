using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenMappingWithOpenGenerics
    {
        [TestMethod]
        public void Map_With_Open_Generics()
        {
            TypeAdapterConfig.GlobalSettings.ForType(typeof(GenericPoco<>), typeof(GenericDto<>))
                .Map("value", "Value");

            var poco = new GenericPoco<int> { Value = 123 };
            var dto = poco.Adapt<GenericDto<int>>();
            dto.value.ShouldBe(poco.Value);
        }

        [TestMethod]
        public void Setting_From_OpenGeneric_Has_No_SideEffect()
        {
            var config = new TypeAdapterConfig();
            config
                .NewConfig(typeof(A<>), typeof(B<>))
                .Map("BProperty", "AProperty");

            var a = new A<C> { AProperty = "A" };
            var c = new C { BProperty = "C" };
            var b = a.Adapt<B<C>>(config); // successful mapping
            var cCopy = c.Adapt<C>(config);
        }

        [TestMethod]
        public void MapOpenGenericsUseInherits()
        {
            TypeAdapterConfig.GlobalSettings
                .ForType(typeof(GenericPoco<>), typeof(GenericDto<>))
                .Map("value", "Value");

            TypeAdapterConfig.GlobalSettings
                .ForType(typeof(DerivedPoco<>), typeof(DerivedDto<>))
                .Map("derivedValue", "DerivedValue")
                .Inherits(typeof(GenericPoco<>), typeof(GenericDto<>));

            var poco = new DerivedPoco<int> { Value = 123 , DerivedValue = 42 };
            var dto = poco.Adapt<DerivedDto<int>>();
            dto.value.ShouldBe(poco.Value);
            dto.derivedValue.ShouldBe(poco.DerivedValue);
        }

        [TestMethod]
        public void MapOpenGenericsUseInclude()
        {
            TypeAdapterConfig.GlobalSettings.Clear();
           
            TypeAdapterConfig.GlobalSettings
                .ForType(typeof(DerivedPoco<>), typeof(DerivedDto<>))
                .Map("derivedValue", "DerivedValue");

            TypeAdapterConfig.GlobalSettings
                .ForType(typeof(GenericPoco<>), typeof(GenericDto<>))
                .Map("value", "Value");

            TypeAdapterConfig.GlobalSettings
               .ForType(typeof(GenericPoco<>), typeof(GenericDto<>))
               .Include(typeof(DerivedPoco<>), typeof(DerivedDto<>));

            var poco = new DerivedPoco<int> { Value = 123, DerivedValue = 42 };
            var dto = poco.Adapt(typeof(GenericPoco<>), typeof(GenericDto<>));

            dto.ShouldBeOfType<DerivedDto<int>>();

            ((DerivedDto<int>)dto).value.ShouldBe(poco.Value);
            ((DerivedDto<int>)dto).derivedValue.ShouldBe(poco.DerivedValue);
        }

        public class DerivedPoco<T> : GenericPoco<T>
        {
            public T DerivedValue { get; set; }
        }

        public class DerivedDto<T> : GenericDto<T>
        {
            public T derivedValue { get; set; }
        }

        public class GenericPoco<T>
        {
            public T Value { get; set; }
        }

        public class GenericDto<T>
        {
            public T value { get; set; }
        }
         
        class A<T> { public string AProperty { get; set; } }

        class B<T> { public string BProperty { get; set; } }

        class C { public string BProperty { get; set; } }
    }
}
