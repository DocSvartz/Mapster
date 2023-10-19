using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenMappingObjectRegression
    {
        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/524 
        /// </summary>
        [TestMethod]
        public void TSourceIsObjectUpdate()
        {
            var source = new Source524 { X1 = 123 };
            var _result = Somemap(source);

            _result.X1.ShouldBe(123);
        }

        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/524
        /// </summary>
        [TestMethod]
        public void TSourceIsObjectUpdateUseDynamicCast()
        {
            var source = new Source524 { X1 = 123 };
            var _result = SomemapWithDynamic(source);

            _result.X1.ShouldBe(123);
        }


        #region TestFunctions

        Dest524 Somemap(object source)
        {
            var dest = new Dest524 { X1 = 321 };
            var dest1 = source.Adapt(dest);

            return dest;
        }

        Dest524 SomemapWithDynamic(object source)
        {
            var dest = new Dest524 { X1 = 321 };
            var dest1 = source.Adapt(dest, source.GetType(), dest.GetType());

            return dest;
        }

        #endregion TestFunctions

        #region TestClasses
        class Source524
        {
            public int X1 { get; set; }
        }
        class Dest524
        {
            public int X1 { get; set; }
        }

        #endregion TestClasses
    }
}
