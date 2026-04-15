using Mapster.Compile.Configuration.Inspectors;
using System;
using System.Collections.Immutable;

namespace Mapster.Compile.Configuration.Matchers
{
    internal class ConstructorMatcher
    {
        public ConstructorMatcher(MethodInspector destiantonCtor)
        {
            DestiantonCtor = destiantonCtor ?? throw new ArgumentNullException(nameof(destiantonCtor));
        }

        public MethodInspector DestiantonCtor { get;}
        public ImmutableArray<ParamMatcher> ParamsMapping { get; }
    }
}
