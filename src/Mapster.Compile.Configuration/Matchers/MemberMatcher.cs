using Mapster.Compile.Config;
using Microsoft.CodeAnalysis;

namespace Mapster.Compile.Configuration.Matchers
{
    public class MemberMatcher : MemberMatcherBase
    {
        public MemberMatcher(ISymbol destinationMember, ISymbol sourceMember, TypeAdapterSettingsCompile settings) : base(destinationMember, sourceMember)
        {
        }

        public ISymbol SourcePayload { get; }
        public bool UseDestinationValue { get; }
    }
}
