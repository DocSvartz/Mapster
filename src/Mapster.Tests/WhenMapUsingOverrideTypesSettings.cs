using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Collections.Generic;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenMapUsingOverrideTypesSettings
    {
        [TestMethod]
        public void OverrideDestinationTramsformIsWorked()
        {
            var config = new TypeAdapterConfig();
            config.Default.AddDestinationTransform(DestinationTransform.EmptyCollectionIfNull);

            config
                .NewConfig<CollectionPocoOverride, CollectionDtoOverride>()
                .MapUsing(src => src.Children, dest => dest.Children,
                cfg =>
                {
                    cfg.SkipAllSettings(true);
                })
                .MapUsing(src => src.Array, dest => dest.Array,
                cfg =>
                {
                    cfg
                        .ReConfigurate()
                        .MapWith(x => x ?? new[] { 42 });
                });


            var source = new CollectionPocoOverride();
            var destination = source.Adapt<CollectionDtoOverride>(config);

            destination.MultiDimentionalArray.Length.ShouldBe(0);
            destination.ChildDict.Count.ShouldBe(0);
            destination.Set.Count.ShouldBe(0);

             
            destination.Children.ShouldBeNull(); // Destination Transforms from global context settings is skipped for this property
            destination.Array[0].ShouldBe(42); // Custom converter for types is worked, Destination Transforms is not achievable because the custom converter never returns null

           
            var destWithNotTypesSettingOverride = new CollectionPocoOverride().Adapt<CollectionDtoWithArray>(config);

            // Destination Transforms correct work from other mapping types
            destWithNotTypesSettingOverride.Array.Length.ShouldBe(0);
        }


        #region TestClasses

        class CollectionPocoWithArray
        {
            public int[] Array { get; set; }
        }

        class CollectionDtoWithArray
        {
            public int[] Array { get; set; }
        }

        class CollectionPocoOverride
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            public List<ChildPoco> Children { get; set; }
            public int[] Array { get; set; }
            public double[,] MultiDimentionalArray { get; set; }
            public Dictionary<string, ChildPoco> ChildDict { get; set; }
            public HashSet<string> Set { get; set; }
        }

        class CollectionDtoOverride
        {
            public Guid Id { get; set; }
            public string Name { get; set; }

            public IReadOnlyList<ChildDto> Children { get; internal set; }
            public int[] Array { get; set; }
            public double[,] MultiDimentionalArray { get; set; }
            public IReadOnlyDictionary<string, ChildDto> ChildDict { get; set; }
            public ISet<string> Set { get; set; }
        }
        #endregion TestClasses
    }
}
