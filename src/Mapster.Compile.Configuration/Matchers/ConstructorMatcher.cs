using Mapster.Compile.Configuration.Inspectors;
using Microsoft.CodeAnalysis;
using System.Collections;
using System.Collections.Immutable;

namespace Mapster.Compile.Configuration.Matchers
{
    internal class ConstructorMatcher
    {
        public MethodInspector DestiantonCtor { get;}
        public ImmutableArray<ParamMatcher> ParamsMapping { get; }
    }
}
