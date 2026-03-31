using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenMappingRequiredPropertyRegression
    {
        [TestMethod]
        public void RequiredProperty()
        {
            var source = new Person553 { FirstMidName = "John", LastName = "Dow" };
            var destination = new Person554 { ID = 245, FirstMidName = "Mary", LastName = "Dow" };

            var s = source.BuildAdapter().CreateMapToTargetExpression<Person554>();

            var result = source.Adapt(destination);

            result.ID.ShouldBe(245);
            result.FirstMidName.ShouldBe(source.FirstMidName);
            result.LastName.ShouldBe(source.LastName);
        }

        [TestMethod]
        public void PolymorphicMappingToAbstractClassCompileWithoutError()
        {
            var config = TypeAdapterConfig.GlobalSettings;

            config.NewConfig<ConcreteSource, ConcreteDestination>();

            config.NewConfig<AbstractSource, AbstractDestination>()
                .Include<ConcreteSource, ConcreteDestination>();

            Should.NotThrow(() =>
            {
                config.Compile();
            });

        }
    }

    #region TestClasses

    public abstract class AbstractSource
    {
        public abstract string Name { get; }
    }

    public class ConcreteSource : AbstractSource
    {
        public override string Name => "Test";
    }

    public abstract class AbstractDestination
    {
        public required string Name { get; set; }
    }

    public class ConcreteDestination : AbstractDestination
    {

    }

    #endregion TestClasses
}
