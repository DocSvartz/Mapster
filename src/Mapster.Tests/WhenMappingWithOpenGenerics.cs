using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System.Collections.Generic;
using static Mapster.Tests.WhenMappingWithOpenGenerics;

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

            config.Compile(); // is not throw exception

            var a = new A<C> { AProperty = "A" };
            var c = new C { BProperty = "C" };
            var b = a.Adapt<B<C>>(config); // successful mapping
            var cCopy = c.Adapt<C>(config);
        }

        

        [TestMethod]
        public void MapOpenGenericsUseInclude()
        {
            TypeAdapterConfig.GlobalSettings.Clear();

            var config = new TypeAdapterConfig();
            config
            .ForType(typeof(DerivedPoco<,,>), typeof(DerivedPoco<,,>))
            .Map("derivedValue", "DerivedValue");


            config.Compile();   


          
        }

        public class DerivedPoco<T,X,Y>
            where T : IList<X>
            where X : Y
            where Y : A1, IActiveBase2, IActivityData
        { 
           public T A { get; set; }
           public X B { get; set; }
           
           public Y C { get; set; }
        }


        public  class A1{ }

        public class B1 { }

        public class C1 : A1 { } 

        public interface IActiveBase2
        {
            public string ActiveBase2 { get; set; }
        }


        public struct Mystruct
        {

        }
               
        public abstract class Activitybase
        {
            public string Hello { get; set; }
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
