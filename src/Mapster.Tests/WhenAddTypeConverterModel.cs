using ExpressionDebugger;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Shouldly;
using System;
using System.Reflection;

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

        [TestMethod]
        public void NotDublicateMemeberInit_WhenRecordUsingCustomInit_WithNewOrMemberinit()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<RecordWithCustomInit, RecordWithCustomInit>()
                .Map(dest => dest.DataReMap, src => src.DataReMap)
                .Ignore(dest=>dest.DataIgnore)
                .ConstructUsing(() => new RecordWithCustomInit("32", "John", "Remap") { OtherData = "World" });

            var src = new RecordWithCustomInit("42", "John", "Src");

            var strMap = src.BuildAdapter(config).CreateMapExpression<RecordWithCustomInit>().ToScript();

            var strMapToTarget = src.BuildAdapter(config).CreateMapToTargetExpression<RecordWithCustomInit>().ToScript();

            // Valid Map/Projection Record mapping pattern
            // Custom init is not create dublicate mapping memebers
            // and Custom memeber mapping using .Map() ovveride  member using in custom init.
            // (ctor param dataRemap ovveride when using .Map())
            // (property OtherData use custom init definition because not using .Map())
            //WhenAddTypeConverterModel.RecordWithCustomInit result = new WhenAddTypeConverterModel.RecordWithCustomInit("32", "John", "Remap")
            //{
            //    OtherData = "World",
            //    Data = p1.Data,
            //    DataReMap = p1.DataReMap
            //};

            strMap.ShouldContain("WhenAddTypeConverterModel.RecordWithCustomInit result = new WhenAddTypeConverterModel.RecordWithCustomInit(\"32\", \"John\", \"Remap\")\r\n    {\r\n        OtherData = \"World\",\r\n        Data = p1.Data,\r\n        DataReMap = p1.DataReMap\r\n    };");

            // Valid MapToTarget Record mapping pattern
            // All maybe members must obtain values ​​from the source.
            // Ignored memebers must be restored
            //WhenAddTypeConverterModel.RecordWithCustomInit result = new WhenAddTypeConverterModel.RecordWithCustomInit("32", "John", "Remap")
            //{
            //    Value = p1.Value,
            //    Name = p1.Name,
            //    OtherData = p1.OtherData,
            //    Data = p1.Data,
            //    DataIgnore = p2 == null ? null : p2.DataIgnore, // restore ignored member
            //    DataReMap = p1.DataReMap
            //};

            strMapToTarget.ShouldContain("WhenAddTypeConverterModel.RecordWithCustomInit result = new WhenAddTypeConverterModel.RecordWithCustomInit(\"32\", \"John\", \"Remap\")\r\n    {\r\n        Value = p1.Value,\r\n        Name = p1.Name,\r\n        OtherData = p1.OtherData,\r\n        Data = p1.Data,\r\n        DataIgnore = p2 == null ? null : p2.DataIgnore,\r\n        DataReMap = p1.DataReMap\r\n    };");
        }

        [TestMethod]
        public void ValidMapping_WhenRecordUsingCustomInit_WithOutNewOrMemberinit()
        {
            var config = new TypeAdapterConfig();
            config.NewConfig<RecordWithCustomInit, RecordWithCustomInit>()
                .Map(dest => dest.DataReMap, src => src.DataReMap)
                .Ignore(dest => dest.DataIgnore)
                .ConstructUsing(() => RecordWithCustomInit.Init());

            var src = new RecordWithCustomInit("42", "John", "Src");

            var strMap = src.BuildAdapter(config).CreateMapExpression<RecordWithCustomInit>().ToScript();

            var strMapToTarget = src.BuildAdapter(config).CreateMapToTargetExpression<RecordWithCustomInit>().ToScript();

            // Valid Map/Projection Record mapping pattern
            // All member must be mapping 
            // Ignored member is not mapping
            //{
            //    WhenAddTypeConverterModel.RecordWithCustomInit result = WhenAddTypeConverterModel.RecordWithCustomInit.Init();
            //    result.Value = p1.Value;
            //    result.Name = p1.Name;
            //    typeof(WhenAddTypeConverterModel.RecordWithCustomInit).GetField("<OtherData>k__BackingField", (BindingFlags)Instance, NonPublic).SetValue(result, p1.OtherData);
            //    result.Data = p1.Data;
            //    result.DataReMap = p1.DataReMap;
            //}

            strMap.ShouldContain("WhenAddTypeConverterModel.RecordWithCustomInit result = WhenAddTypeConverterModel.RecordWithCustomInit.Init();\r\n    \r\n    result.Value = p1.Value;\r\n    result.Name = p1.Name;\r\n    typeof(WhenAddTypeConverterModel.RecordWithCustomInit).GetField(\"<OtherData>k__BackingField\", (BindingFlags)Instance, NonPublic).SetValue(result, p1.OtherData);\r\n    result.Data = p1.Data;\r\n    result.DataReMap = p1.DataReMap;");

            // Valid MapToTarget Record mapping pattern
            // All maybe members must obtain values ​​from the source.
            // Ignored memebers must be restored
            //{
            //    WhenAddTypeConverterModel.RecordWithCustomInit result = WhenAddTypeConverterModel.RecordWithCustomInit.Init();
            //    result.Value = p1.Value;
            //    result.Name = p1.Name;
            //    typeof(WhenAddTypeConverterModel.RecordWithCustomInit).GetField("<OtherData>k__BackingField", (BindingFlags)Instance, NonPublic).SetValue(result, p1.OtherData);
            //    result.Data = p1.Data;
            //    result.DataIgnore = p2 == null ? null : p2.DataIgnore;
            //    result.DataReMap = p1.DataReMap;
            //    return result;
            //}

            strMapToTarget.ShouldContain("WhenAddTypeConverterModel.RecordWithCustomInit result = WhenAddTypeConverterModel.RecordWithCustomInit.Init();\r\n    \r\n    result.Value = p1.Value;\r\n    result.Name = p1.Name;\r\n    typeof(WhenAddTypeConverterModel.RecordWithCustomInit).GetField(\"<OtherData>k__BackingField\", (BindingFlags)Instance, NonPublic).SetValue(result, p1.OtherData);\r\n    result.Data = p1.Data;\r\n    result.DataIgnore = p2 == null ? null : p2.DataIgnore;\r\n    result.DataReMap = p1.DataReMap;\r\n    return result;");
        }


        #region TestClasses

        internal record RecordWithCustomInit
        {
            public RecordWithCustomInit(string value, string name, string dataReMap)
            {
                Value = value;
                Name = name;
                DataReMap = dataReMap;
            }

            public string Value { get; private set; }

            public string Name { get; private set; }
            public string OtherData { get; init; }
            public string Data { get; private set; }
            public string DataIgnore { get; private set; }
            public string DataReMap { get; private set; }

            public static RecordWithCustomInit Init() => new RecordWithCustomInit("34", "Mark", "RemapBase");
        }

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
