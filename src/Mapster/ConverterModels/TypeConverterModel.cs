using Mapster.Utils;
using System.Linq.Expressions;

namespace Mapster.MappingModels
{
    internal class TypeConverterModel
    {
        private bool? _isNewOrMemberInitCustomExp;
        (string[] ctorParamsName, MemberBinding[] memberBindings)? _customInitInfo;

        public Expression? CustomInitExpression { get; set; }
        public ConstructorConveterModel? ConstructorMapping { get; set; }
        public PropertyOrFielldConveterModel PropertyOrFieldMapping { get; set; }

        private bool CheckCustomInit()
        {
            if (_isNewOrMemberInitCustomExp != null)
                return _isNewOrMemberInitCustomExp.GetValueOrDefault();

            if (CustomInitExpression is null)
                _isNewOrMemberInitCustomExp = false;

            _isNewOrMemberInitCustomExp = CustomInitExpression!.IsNewOrMemberMemberInitExpression(out _customInitInfo);


            return _isNewOrMemberInitCustomExp.GetValueOrDefault();
        }

        public bool IsCustomInit => ConstructorMapping == null;
        public bool IsNewOrMemberInit  => IsCustomInit ? CheckCustomInit() : true;

    }
}
