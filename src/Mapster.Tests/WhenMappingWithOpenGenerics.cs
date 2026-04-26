using Mapster.Models;
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

        [TestMethod]
        public void WhenMapIOpenGenericSettingsWorked()
        {
            var config = new TypeAdapterConfig();

            config
                .NewConfig<ClassA<IOpenGeneric>, ClassB<IOpenGeneric>>()
                .Map(dest => dest.Variable2, src => src.Variable);

            config.Compile();

            var classA = new ClassA<int> { Variable = 15 };

            var classB = classA.Adapt<ClassB<int>>(config);
            var result2 = classA.Adapt<ClassA<int>, ClassB<string>>(config);

            classB.Variable2.ShouldBe(15);
            result2.Variable2.ShouldBe("15");
        }

        [TestMethod]
        public void MapIOpenGenericsUseInherits()
        {
            var config = new TypeAdapterConfig();
            config
                .NewConfig<GenericPoco<IOpenGeneric>, GenericDto<IOpenGeneric>>()
                .Map(dest => dest.value, src => src.Value);

            config
                .NewConfig<DerivedPoco<IOpenGeneric>, DerivedDto<IOpenGeneric>>()
                .Map(dest => dest.derivedValue, src => src.DerivedValue)
                .Inherits<GenericPoco<IOpenGeneric>, GenericDto<IOpenGeneric>>();

            config.Compile();

            var poco = new DerivedPoco<int> { Value = 123, DerivedValue = 42 };
            var dto = poco.Adapt<DerivedDto<int>>(config);
            dto.value.ShouldBe(poco.Value);
            dto.derivedValue.ShouldBe(poco.DerivedValue);
        }

        [TestMethod]
        public void MapIOpenGenericsUseInclude()
        {
            var config = new TypeAdapterConfig();
           
            config
                .NewConfig<DerivedPoco<IOpenGeneric>, DerivedDto<IOpenGeneric>>()
                .Map(dest => dest.derivedValue, src => src.DerivedValue);

            config
                .NewConfig<GenericPoco<IOpenGeneric>, GenericDto<IOpenGeneric>>()
                .Map(dest => dest.value, src => src.Value)
                .Include<DerivedPoco<IOpenGeneric>, DerivedDto<IOpenGeneric>>();

            config.Compile();

            var poco = new DerivedPoco<int> { Value = 123, DerivedValue = 42 };

            var dto = poco.Adapt<GenericPoco<IOpenGeneric>, GenericDto<IOpenGeneric>>(config);

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
        class ClassA<T> { public T? Variable { get; set; } = default; }
        class ClassB<T> { public T? Variable2 { get; set; } = default; }
    }


    public static class adaptHelper
    {
        public static object? AdaptOpenGEneric<TSource,TDestination>(this object source, TypeAdapterConfig config)
        {
            if (typeof(TSource).IsOpenGenericType() && typeof(TDestination).IsOpenGenericType())
            {
                TypeAdapterRule rule;
                    config.RuleMap.TryGetValue(new TypeTuple(typeof(TSource), typeof(TDestination)), out rule);

                if(rule != null)
                {
                    foreach (var item in rule.Settings.Includes)
                    {
                        if (source.GetType().GetGenericTypeDefinition().BaseType.IsAssignableFrom(item.Source.GetGenericTypeDefinition().BaseType))
                        {

                            TypeAdapterRule getCurrentSettings;
                                config.RuleMap.TryGetValue(new TypeTuple(source.GetType().GetGenericTypeDefinition().MakeGenericType(rule.Settings.SourceType.GetGenericArguments()), item.Destination.GetGenericTypeDefinition().MakeGenericType(rule.Settings.DestinationType.GenericTypeArguments)), out getCurrentSettings);

                            if (rule != null)
                                getCurrentSettings.Settings.Apply(rule.Settings);


                            return source.Adapt(source.GetType(), item.Destination.GetGenericTypeDefinition().MakeGenericType(source.GetType().GenericTypeArguments), config);
                        }
                    }
                }
            }


            return new object();
        } 

    }
}
