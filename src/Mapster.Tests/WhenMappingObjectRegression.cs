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
        public void TSousreIsObjectUpdate()
        {
            var source = new TestClassPublicCtr { X = 123 };
            var _result = Somemap(source);

            _result.X.ShouldBe(123);
        }

        TestClassPublicCtr Somemap(object source)
        {
            var dest = new TestClassPublicCtr { X = 321 };
            var dest1 = source.Adapt(dest);

            return dest;
        }

        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/524
        /// </summary>
        [TestMethod]
        public void TSousreIsObjectUpdateUseDynamicCast()
        {
            var source = new TestClassPublicCtr { X = 123 };
            var _result = SomemapWithDynamic(source);

            _result.X.ShouldBe(123);
        }

        TestClassPublicCtr SomemapWithDynamic(object source)
        {
            var dest = new TestClassPublicCtr { X = 321 };
            var dest1 = source.Adapt(dest, source.GetType(), dest.GetType());

            return dest;
        }
    }
}
