using Microsoft.CodeAnalysis;

namespace Mapster.Compile.Configuration.Matchers
{
    internal class MemberMatcherBase
    {
        public ISymbol DestinationMember { get; }
        public ISymbol SourceMember { get; }
    }
}