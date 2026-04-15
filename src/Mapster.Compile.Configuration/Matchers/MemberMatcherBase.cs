using Microsoft.CodeAnalysis;
using System;

namespace Mapster.Compile.Configuration.Matchers
{
    public abstract class MemberMatcherBase
    {
        protected MemberMatcherBase(ISymbol destinationMember, ISymbol sourceMember)
        {
            DestinationMember = destinationMember ?? throw new ArgumentNullException(nameof(destinationMember));
            SourceMember = sourceMember ?? throw new ArgumentNullException(nameof(sourceMember));
        }

        public ISymbol DestinationMember { get; }
        public ISymbol SourceMember { get; }
    }
}