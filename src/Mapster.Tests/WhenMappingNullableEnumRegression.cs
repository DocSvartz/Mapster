using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.DirectoryServices;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenMappingNullableEnumRegression
    {
        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/640
        /// </summary>
        [Ignore]
        [TestMethod]
        public void NullEnumToNullClass()
        {
            TypeAdapterConfig<Enum?, KeyValueData?>
                .NewConfig()
                .MapWith(s => s == null ? null : new KeyValueData(s.ToString(), Enums.Manager));

            MyClass myClass = new() { TypeEmployer = MyEnum.User };

            var _result = myClass?.Adapt<MyDestination?>(); // Work

            _result.TypeEmployer.Key.ShouldBe(MyEnum.User.ToString());
        }

        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/640
        /// </summary>
        [Ignore] // Will work after RecordType fix 
        [TestMethod]
        public void UpdateNullEnumToClass()
        {
            TypeAdapterConfig<Enum?, KeyValueData?>
                .NewConfig()
                .MapWith(s => s == null ? null : new KeyValueData(s.ToString(), Enums.Manager));

          
            MyClass myClass = new() { TypeEmployer = MyEnum.User };

            var mDest2 = new MyDestination() { TypeEmployer = new KeyValueData("Admin", null) };
           
            var _MyDestination = myClass?.Adapt<MyDestination?>(); // Work
            var _result = _MyDestination.Adapt(mDest2);

            _result.TypeEmployer.Key.ShouldBe(MyEnum.User.ToString());
        }

        [TestMethod]
        public void OptionalT()
        {
            TypeAdapterConfig<Optional561<string>, string>
                .ForType()
                .MapToTargetPrimitive((source, target) => source.HasValue ? source.Value : target)
                .IgnoreNullValues(true);

           

            var sourceNull = new Source561 { Name = new Optional561<string?>(null) };

            var target = new Source561 { Name = new Optional561<string>("John") }.Adapt<Target561>();

           
           var TargetDestinationFromNull = new Target561() { Name = "Me" };

          var NullToupdateoptional = sourceNull.Adapt(TargetDestinationFromNull);

           var _result = sourceNull.Adapt(target);

            target.Name.ShouldBe("John");
        }



    }

    #region TestClasses

    [AdaptToPrimitive(typeof(string))]
    class Optional561<T>
    {
        
        public Optional561(T? value) 
        {
            if (value != null)
                HasValue = true;

            Value = value;

          
        }

        public bool HasValue { get; }
        public T? Value { get; }
    }

    class Source561
    {
        public Optional561<string?> Name { get; set; }
    }

    class Target561
    {
        public string Name { get; set; }
    }


    class MyDestination
    {
        public KeyValueData? TypeEmployer { get; set; }
    }

    class MyClass
    {
        public MyEnum? TypeEmployer { get; set; }
    }

    enum MyEnum
    {
        Anonymous = 0,
        User = 2,
    }

    class FakeResourceManager
    {

    }

    class Enums
    {
        protected Enums(string data) {}

        public static FakeResourceManager Manager { get; set; }

    }

    record KeyValueData
    {
        private readonly string? keyHolder; 
        private string? description;

        public KeyValueData(string key, FakeResourceManager manager)
        {
            this.keyHolder = key?.ToString();
            Description = manager?.ToString();
        }

        public string Key
        {
            get => keyHolder!;
            set { } 
        }

        public string? Description
        {
            get => description;
            set => description ??= value;
        }
    }


    #endregion TestClasses
}
