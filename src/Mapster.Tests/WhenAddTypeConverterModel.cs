using ExpressionDebugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenAddTypeConverterModel
    {
        [TestMethod]
        public void NotDublicateMemeberInitWhenCtorUsingMembers_WhenTypeActivateUsingConstuctor()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<CtorInitMember, CtorInitMember>()
                .Ignore(dest=>dest.OtherData);

            var src = new CtorInitMember("42","John");

            var str = src.BuildAdapter(config).CreateMapExpression<CtorInitMember>().ToString();

            str.ShouldContain("new CtorInitMember(p1.Value, p1.Name) {Data = p1.Data}");

        }

        [TestMethod]
        public void NotDublicateMemeberInit_WhenTypeActivateUsingConstuct_WithNewOrMemberinit()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<CtorInitMember, CtorInitMember>()
                .ConstructUsing(()=> new CtorInitMember("32", "John") { OtherData = "World" });

            var src = new CtorInitMember("42", "John");

            var str = src.BuildAdapter(config).CreateMapExpression<CtorInitMember>().ToString();

            str.ShouldContain("new CtorInitMember(\"32\", \"John\") {OtherData = \"World\", Data = p1.Data}");

        }

        [TestMethod]
        public void DublicateMemeberInit_IfHasCustomMapping_WhenTypeActivateUsingConstuct_WithNewOrMemberinit()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<CtorInitMember, CtorInitMember>()
                .ConstructUsing(() => new CtorInitMember("32", "John"))
                .Map(x => x.Value, y => y.Value)
                .Map(x => x.Name, y => y.Name)
                .Ignore(dest=>dest.OtherData);

            var src = new CtorInitMember("42", "John");

            var str = src.BuildAdapter(config).CreateMapExpression<CtorInitMember>().ToString();

            str.ShouldContain("new CtorInitMember(\"32\", \"John\") {Value = p1.Value, Name = p1.Name, Data = p1.Data}");
        }

        [TestMethod]
        public void MemeberInit_WhenTypeActivateUsingConstuct_WithNotNewOrMemberinit()
        {
            var config = new TypeAdapterConfig();
            config.SelfContainedCodeGeneration = true;
            config.NewConfig<CtorInitMember, CtorInitMember>()
                .ConstructUsing(() => CtorInitMember.Init());

            var src = new CtorInitMember("42", "John");

            var str = src.BuildAdapter(config).CreateMapExpression<CtorInitMember>().ToScript();

            str.ShouldSatisfyAllConditions(() =>
            {
                str.ShouldContain("result = WhenAddTypeConverterModel.CtorInitMember.Init()");
                str.ShouldContain("result.Value = p1.Value;");
                str.ShouldContain("result.Name = p1.Name;");
                str.ShouldContain("result.Data = p1.Data;");
            });

        }

        [TestMethod]
        public void NotPublicCtorType_ThrowError_WhenNotCustomCtorMappingConfigurate()
        {

            Should.Throw<InvalidOperationException>(() =>
            {
                TypeAdapterConfig<NotPublicCtorType, NotPublicCtorType>
                    .NewConfig().Compile();
            });
        }

        [TestMethod]
        public void NotPublicCtorType_NotError_WhenCustomCtorMappingConfigurate()
        {

            Should.NotThrow(() =>
            {
                TypeAdapterConfig<NotPublicCtorType, NotPublicCtorType>
                    .NewConfig()
                    .EnableNonPublicConstructors(true)
                    .Compile();

                var result = NotPublicCtorType.Init.Adapt<NotPublicCtorType>();
            });
        }

        [TestMethod]
        public void NotInitType_ThrowError_WhenNotCustomInitMappingConfigurate()
        {
            Exception? result = null;

            try
            {
                var config = new TypeAdapterConfig();

                config.NewConfig<AbstractBase, IBase>().Compile();
                config.NewConfig<IBase, AbstractBase>().Compile();
            }
            catch (Exception error)
            {
               result = error;

                result.ShouldSatisfyAllConditions(() =>
                {
                    result.ShouldBeOfType<CompileException>();
                    result.InnerException.Message.Contains("is Non-instantiable type (Abstract or Interface)").ShouldBeTrue();
                });
               
            }

            result.ShouldBeOfType<CompileException>();
        }

        [TestMethod]
        public void NotInitType_NotError_WhenCustomInitMappingConfigurate()
        {
            Should.NotThrow(() => 
            {
                var config = new TypeAdapterConfig();

                config.ForDestinationType<IBase>()
                    .ConstructUsing(() => new BaseIml());
                config.ForDestinationType<AbstractBase>()
                    .ConstructUsing(() => new BaseIml());


                config.NewConfig<AbstractBase, IBase>();
                config.NewConfig<IBase, AbstractBase>();

                config.Compile();

            });
        }



        #region TestClasses

        internal class BaseIml: AbstractBase, IBase
        {

        }

        public interface IBase
        {
            public string IAmPoco { get; set; }
        }

        internal abstract class AbstractBase
        {
            public string IAmPoco {  get; set; }
        }

        internal class NotPublicCtorType
        {
            private NotPublicCtorType()
            {

            }

            public bool IsAmPoco { get; set; }

            public static NotPublicCtorType Init => new NotPublicCtorType() { IsAmPoco = true };
        }

        internal class CtorInitMember
        {
            public CtorInitMember(string value, string name)
            {
                Value = value;
                Name = name;
            }

            public string Value {  get; private set; }

            public string Name { get; private set; }
            public string OtherData { get; init; }
            public string Data { get; private set; }   

            public static CtorInitMember Init() => new CtorInitMember("34", "Mark");
        }


        #endregion TestClasses
    }
}
