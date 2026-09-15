using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mapster.Models
{
    public class MemberMapping
    {
        public Expression Getter;
        public List<GetterLine> GetterLines { get; } = new List<GetterLine>();
        public IMemberModelEx DestinationMember;
        public Expression? RestoreDestinationMemberExp;
        public IgnoreDictionary.IgnoreItem Ignore;
        public List<InvokerModel> NextResolvers;
        public IgnoreDictionary NextIgnore;
        public ParameterExpression Source;
        public ParameterExpression? Destination;
        public bool UseDestinationValue;
        public TypeAdapterSettings? OverrideSettings;

        public bool HasSettings()
        {
            return NextResolvers.Count > 0 || NextIgnore.Count > 0;
        }
    }

    public class GetterLine
    {
        public GetterLine(Expression source, Expression? getter, TypeAdapterSettings? overrideSettings)
        {
            Source = source;
            Getter = getter;
            OverrideSettings = overrideSettings;
        }

        public Expression Source {  get; init; }
        public Expression? Getter {  get; init; }
        public TypeAdapterSettings? OverrideSettings { get; init; }
       
        public Expression? Condition { get; set; }
        public Expression? NullPropagationChecker { get; set; }
        public Expression? TransformFunc { get; set; }
        
    }
}