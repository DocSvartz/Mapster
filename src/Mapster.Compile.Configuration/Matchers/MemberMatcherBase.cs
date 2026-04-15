using Microsoft.CodeAnalysis;

namespace Mapster.Compile.Configuration.Matchers
{
    public abstract class MemberMatcherBase
    {
        public ISymbol DestinationMember { get; }
        public ISymbol SourceMember { get; }
    }
}