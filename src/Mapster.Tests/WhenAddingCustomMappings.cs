using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using static Mapster.Tests.WhenMappingDerived;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenAddingCustomMappings
    {
        [TestMethod]
        public void Property_Is_Mapped_To_Different_Property_Successfully()
        {
            TypeAdapterConfig<SimplePoco, SimpleDto>.NewConfig()
                .Map(dest => dest.AnotherName, src => src.Name)
                .Map(dest => dest.LastModified, src => DateTime.Now)
                .Map(dest => dest.FileData, src => new FileData { Content = src.FileContent })
                .Compile();

            var poco = new SimplePoco {Id = Guid.NewGuid(), Name = "TestName", FileContent = "Foo"};

            var dto = TypeAdapter.Adapt<SimplePoco, SimpleDto>(poco);

            dto.Id.ShouldBe(poco.Id);
            dto.Name.ShouldBe(poco.Name);
            dto.AnotherName.ShouldBe(poco.Name);
            dto.LastModified.Ticks.ShouldBeGreaterThan(0);
            dto.FileData.Content.ShouldBe("Foo");
        }

        [TestMethod]
        public void Property_Is_Mapped_From_Null_Value_Successfully()
        {
            TypeAdapterConfig<SimplePoco, SimpleDto>.NewConfig()
                .Map(dest => dest.AnotherName, src => (string)null)
                .Compile();

            var poco = new SimplePoco { Id = Guid.NewGuid(), Name = "TestName" };

            var dto = TypeAdapter.Adapt<SimplePoco, SimpleDto>(poco);

            dto.Id.ShouldBe(poco.Id);
            dto.Name.ShouldBe(poco.Name);
            dto.AnotherName.ShouldBeNull();
        }

        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/980
        /// </summary>
        [TestMethod]
        public void ExtraSourceUsingCustomResolverSuccessfully()
        {
            TypeAdapterConfig config = new();
            config.NewConfig<Entity980, Dto980>().Map(e => e, e => e.Props);
            config.NewConfig<List<EntityProp980>, Dto980>()
              .Map(e => e.Address, e => e.Get("Address"))
              .Map(e => e.Description, e => e.Get("Description"))
              .Map(e => e.Phone, e => e.Get("Phone"));

            Entity980 ent = new()
            {
                Name = "My entity",
                Props = new(),
            };

            ent.Props.Add(new() { Key = "Phone", Value = "12345678" });
            ent.Props.Add(new() { Key = "Address", Value = "Default street" });
            ent.Props.Add(new() { Key = "Description", Value = "Sample text" });


            var dto = ent.Adapt<Dto980>(config);
            
            dto.Phone.ShouldBe("12345678");
            dto.Address.ShouldBe("Default street");
            dto.Description.ShouldBe("Sample text");
        }

        #region TestClasses

        
       


        public class SimplePoco
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string FileContent { get; set; }
        }

        public class SimpleDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            public string AnotherName { get; set; }
            public DateTime LastModified { get; set; }
            public FileData FileData { get; set; }
        }

        public class FileData
        {
            public string Content { get; set; }
        }

        public class ChildPoco
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class ChildDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        public class CollectionPoco
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            public List<ChildPoco> Children { get; set; }
        }

        public class CollectionDto
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            public IReadOnlyList<ChildDto> Children { get; internal set; }
        }

        #endregion
    }
    class Entity980
    {
        public string Name { get; set; } = default!;
        public List<EntityProp980> Props { get; set; } = default!;
    }

    public class EntityProp980
    {
        public string Key { get; set; } = default!;
        public string Value { get; set; } = default!;
    }

    public class Dto980
    {
        public string Name { get; set; } = default!;
        public string? Address { get; set; }
        public string? Description { get; set; }
        public string? Phone { get; set; }
    }

    public static class ListExtensions980
    {
        // utility method as expression bodies are not allowed to have null propagating operator
        public static string? Get(this List<EntityProp980> list, string key)
          => list.FirstOrDefault(e => e.Key == key)?.Value;
    }
}