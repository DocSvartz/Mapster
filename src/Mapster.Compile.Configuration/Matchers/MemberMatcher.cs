using Microsoft.CodeAnalysis;

namespace Mapster.Compile.Configuration.Matchers
{
    internal class MemberMatcher : MemberMatcherBase
    {
        public ISymbol SourcePayload { get; }
        public bool UseDestinationValue { get; }
    }
}
