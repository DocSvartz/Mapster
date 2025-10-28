using System;
using System.Linq;
using Mapster.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenCloningConfig
    {
        [TestCleanup]
        public void TestCleanup()
        {
            TypeAdapterConfigFactory.GlobalSettings.Clear();
        }

        [TestMethod]
        public void Alter_Config_After_Map_Should_Error()
        {
            TypeAdapterConfig<SimplePoco, SimpleDto>.NewConfig()
                .Map(dest => dest.Name, src => "a");

            var poco = new SimplePoco
            {
                Id = Guid.NewGuid(),
                Name = "test",
            };
            var result = TypeAdapter.Adapt<SimpleDto>(poco);
            result.Name.ShouldBe("a");

            var ex = Should.Throw<InvalidOperationException>(() =>
                TypeAdapterConfig<SimplePoco, SimpleDto>.ForType()
                    .Map(dest => dest.Name, src => "b"));
            ex.Message.ShouldContain("TypeAdapter.Adapt was already called");
        }

        [TestMethod]
        public void Clone()
        {
            TypeAdapterConfig<SimplePoco, SimpleDto>.NewConfig()
                .Map(dest => dest.Name, src => "a");

            var poco = new SimplePoco
            {
                Id = Guid.NewGuid(),
                Name = "test",
            };
            var result = TypeAdapter.Adapt<SimpleDto>(poco);
            result.Name.ShouldBe("a");

            var config = TypeAdapterConfigFactory.GlobalSettings.Clone();
            var global = TypeAdapterConfigFactory.GlobalSettings;
            config.ShouldNotBeSameAs(global);
            config.Default().ShouldNotBeSameAs(global.Default());
            config.Default().Settings.ShouldNotBeSameAs(global.Default().Settings);
            config.RuleMap.ShouldNotBeSameAs(global.RuleMap);
            foreach (var kvp in config.RuleMap)
            {
                var globalRule = global.RuleMap[kvp.Key];
                kvp.Value.ShouldNotBeSameAs(globalRule);
                kvp.Value.Settings.ShouldNotBeSameAs(globalRule.Settings);
            }
            config.GetRules((input)=> true).ShouldNotBeSameAs(global.GetRules((input) => true));

            var rules = config.GetRules((input) => true).ToArray();
            var globalrules = global.GetRules((input) => true).ToArray();

            for (var i = 0; i < rules.Count(); i++)
            {
                rules[i].ShouldNotBeSameAs(globalrules[i]);
                rules[i].Settings.ShouldNotBeSameAs(globalrules[i].Settings);
            }
            rules.Any(rule => object.ReferenceEquals(rule.Settings, config.Default().Settings)).ShouldBeTrue();
            rules.ShouldContain(config.RuleMap[new TypeTuple(typeof(SimplePoco), typeof(SimpleDto))]);
        }

        public class SimplePoco
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class SimpleDto
        {
            public Guid Id { get; set; }
            public string Name { get; internal set; }
        }
    }
}
