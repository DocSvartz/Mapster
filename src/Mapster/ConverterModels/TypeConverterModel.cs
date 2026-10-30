using System.Linq.Expressions;

namespace Mapster.MappingModels
{
    internal class TypeConverterModel
    {
        public Expression? CustomInitExpression { get; set; }
        public ConstructorConveterModel? ConstructorMapping { get; set; }
        public PropertyOrFielldConveterModel PropertyOrFieldMapping { get; set; }
    }
}
