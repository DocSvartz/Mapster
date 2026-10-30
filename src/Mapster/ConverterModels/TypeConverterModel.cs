using Mapster.Utils;
using System.Linq.Expressions;

namespace Mapster.MappingModels
{
    internal class TypeConverterModel
    {
        private bool? _isNewOrMemberInit;
        (string[] ctorParamsName, string[]? memberBindingsName)? _customInitMemberNames;

        public Expression? CustomInitExpression { get; set; }
        public ConstructorConveterModel? ConstructorMapping { get; set; }
        public PropertyOrFielldConveterModel PropertyOrFieldMapping { get; set; }

        private bool CheckCustomInit()
        {
            if (_isNewOrMemberInit != null)
                return _isNewOrMemberInit.GetValueOrDefault();

            if (CustomInitExpression is null)
                _isNewOrMemberInit = false;

            _isNewOrMemberInit = CustomInitExpression!.IsNewOrMemberMemberInitExpression(out _customInitMemberNames);


            return _isNewOrMemberInit.GetValueOrDefault();
        }

        public bool IsNewOrMemberInitCustomExpr { get => CheckCustomInit(); }

        public (string[] ctorParamsName, string[]? memberBindingsName)? CustomInitMemberNames  
            { get => IsNewOrMemberInitCustomExpr ? _customInitMemberNames : _customInitMemberNames;}
    }
}
