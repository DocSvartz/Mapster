using System.IO;

namespace ExpressionDebugger
{
    public static class GeneratedAttributeHelpers
    {
        public static string NameSpace = "MapsterHelpersGenerated";
        public static string CreateGenerateMapsterAttribute =
            "using System;\r\n\r\nnamespace MapsterHelpersGenerated\r\n{\r\n    public sealed class MapsterToolMapperCreatedAttribute : Attribute\r\n    {\r\n\r\n    }\r\n} ";

        public static void WriteFile(string code, string path)
        {
            var dir = Path.GetDirectoryName(path);
            if (dir != null)
                Directory.CreateDirectory(dir);
            if (File.Exists(path))
            {
                var old = File.ReadAllText(path);
                if (old == code)
                    return;
            }
            File.WriteAllText(path, code);
        }

    }
}
