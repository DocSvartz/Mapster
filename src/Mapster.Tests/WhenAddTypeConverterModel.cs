using ExpressionDebugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;

namespace Mapster.Tests
{
    [TestClass]
    public class WhenAddTypeConverterModel
    {
        [TestMethod]
        public void NotDublicateMemeberInitWhenCtorUsingMembers_WhenTypeActivateUsingConstuctor()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<CtorInitMember, CtorInitMember>();

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
                .ConstructUsing(() => new CtorInitMember("32", "John") { OtherData = "World"})
                .Map(x => x.Value, y => y.Value)
                .Map(x => x.Name, y => y.Name);

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



        #region TestClasses



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
