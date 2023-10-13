using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenMappingNullableRegression
    {
        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/640
        /// </summary>
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

        /// <summary>
        /// https://github.com/MapsterMapper/Mapster/issues/476
        /// </summary>
        [TestMethod]
        public void UpdateNullStringEmail()
        {
            TypeAdapterConfig<string, Email>
                .NewConfig()
                .MapWith(value => new Email(value));

            TypeAdapterConfig<Email, string>
                .NewConfig()
                .MapWith( src=> src.Value );


            PostManUser postManUser = new () { Email = new Email("123@gmail.com") };
            PostManUser postManUserWithNullEmail = new() { Email = null };
            PostManDto postManDtoWithNullEmail = new PostManDto() { Email = null };
            PostManDto postManDto = new() { Email = "234@gmail.com" };

            var resultDto = postManUser.Adapt<PostManDto>(); 
            var resultUser = postManDto.Adapt<PostManUser>();
            var updateDto = postManUser.Adapt(postManDtoWithNullEmail); 
            var updateUser = postManDto.Adapt(postManUserWithNullEmail);

            resultDto.Email.ShouldBe("123@gmail.com");
            resultUser.Email.Value.ShouldBe("234@gmail.com");
            updateDto.Email.ShouldBe("123@gmail.com");
            updateUser.Email.Value.ShouldBe("234@gmail.com");

        }

        
    }


    #region TestClasses

    class PostManUser
    {
        public Email Email { get; set; }
    }

    class PostManDto
    {
        public string Email { get; set; }   
    }

    class Email
    {
        public Email(string value ) { Value = value;  }
        
        public string Value { get; protected set; }
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
