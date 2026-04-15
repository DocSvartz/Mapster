using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Mapster.Compile.Configuration.Inspectors
{
    public class MethodInspector
    {
        public INamedTypeSymbol Method { get; }
        public bool IsPartial { get; }
        public bool IsConstuctor { get; }
        public IEnumerable<ISymbol> Params { get; }

    }
}
