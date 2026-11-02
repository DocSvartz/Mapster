using Mapster.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mapster.MappingModels
{
    internal class TypeConverterModel
    {
        public TypeConverterModel(TypeAdapterSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        private readonly TypeAdapterSettings _settings;
        private Func<string, string> ParameterNameConverter => _settings.NameMatchingStrategy.ParameterNameConverter;
        private bool? _isNewOrMemberInitCustomExp;
        (NewExpression newExp, MemberBinding[] memberBindings)? _customInitInfo;
        private HashSet<string>? TransformInitMembers = null;
                
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

        /// <summary>
        /// Check for the use of this member within a constructor or a member initialization expression
        /// </summary>
        /// <param name="memberName"></param>
        /// <returns></returns>
        public bool IsUsingInInit(string destinationMemberName)
        {
            if(!IsNewOrMemberInit)
                return false;
            if(!IsCustomInit)
            {
                if(TransformInitMembers == null)
                    TransformInitMembers = 
                        new HashSet<string>(ConstructorMapping!.Members
                            .Select(x=> ParameterNameConverter(x.DestinationMember.Name)));

               return TransformInitMembers.Contains(ParameterNameConverter(destinationMemberName));
            }
            else
            {
                if (TransformInitMembers == null)
                    TransformInitMembers =
                        new HashSet<string>(_customInitInfo!.Value.newExp.GetNewExpressionParamsName().Select(x => ParameterNameConverter(x))
                        .Concat(_customInitInfo!.Value.memberBindings.Select(x=> ParameterNameConverter(x.Member.Name))));

                return TransformInitMembers.Contains(ParameterNameConverter(destinationMemberName));
            }
        }

        public MemberBinding[] GetInitExpMemberBindings =>
            IsNewOrMemberInit == false || !IsCustomInit ? Array.Empty<MemberBinding>()
            : _customInitInfo!.Value.memberBindings;
        public NewExpression? GetCustomNewExp => IsCustomInit && IsNewOrMemberInit ? _customInitInfo!.Value.newExp : null;
    }
}
