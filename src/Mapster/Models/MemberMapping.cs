using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mapster.Models
{
    public class MemberMapping
    {
        public Expression Getter;
        public List<GetterLine> GetterLines { get; } = new List<GetterLine>();
        public IMemberModelEx DestinationMember;
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
        public Expression? Source {  get; set; }
        public Expression? Condition { get; set; }
        public Expression? NullPropagationChecker { get; set; }
        public TypeAdapterSettings? OverrideSettings;
    }
}