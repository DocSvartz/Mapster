using System.Text;

namespace ExpressionDebugger.Helpers.GeneratedAttributes
{
    public class MapsterToolGeneratedMapperAttribute : GeneratedBase, IGeneratedAttribute
    {
        private readonly StringBuilder _Declaration;
        private readonly bool _isRandomNameSpace;
        private readonly string _NameSpace;

        public string NameSpace => _NameSpace;

        public string Declaration => _Declaration.ToString();

        public string Implimentation => "[MapsterToolGeneratedMapper]";

        public string FileName => "MapsterToolGeneratedMapperAttribute";

       public MapsterToolGeneratedMapperAttribute(bool isRandomNameSpace = false)
       {
            _isRandomNameSpace = isRandomNameSpace;

            if (_isRandomNameSpace)
                _NameSpace = $"Mapster.Generated.Attributes.{RandomNamespaceGenerator.Generate(1,1)}";
            else
                _NameSpace = "Mapster.Generated.Attributes";

            _Declaration = new StringBuilder();

            _Declaration.Append("using System;\r\n\r\n");
            _Declaration.Append($"namespace {NameSpace}");
            _Declaration.Append("\r\n{\r\n    public sealed class MapsterToolGeneratedMapperAttribute : Attribute\r\n    {\r\n\r\n    }\r\n} ");
        }

    }
}
