namespace Mapster.Tool.Tests
{
    public class MapsterToolConfigTests : TestBase
    {
        [Fact]
        public void PrintFullTypeName_option_GenerateCodeWithGlobalNamespacePreffix()
        {
            var mappersWithGlobal = new List<string>();
            var mapperConfig = ConfigHelpers.optMappers;

            mapperConfig.PrintFullTypeName = true;

            Generators.GenerateMappers(mapperConfig, mappersWithGlobal);

            CompileValidator.TryCompile(mappersWithGlobal[0], out string Errors);
        }
    }
}
