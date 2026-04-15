using Mapster.Compile.Config;
using Microsoft.CodeAnalysis;

namespace Mapster.Compile.Configuration.Matchers
{
    internal class ParamMatcher : MemberMatcherBase
    {
        public ParamMatcher(ISymbol destinationMember, ISymbol sourceMember, TypeAdapterSettingsCompile settings) : base(destinationMember, sourceMember)
        {
        }
    }
}
