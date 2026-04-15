using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;

namespace Mapster.Compile.Configuration.Inspectors
{
    public class MethodInspector
    {
        public MethodInspector(INamedTypeSymbol method)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
        }

        public INamedTypeSymbol Method { get; }
        public bool IsPartial { get; }
        public bool IsConstuctor { get; }
        public IEnumerable<ISymbol> Params { get; }

    }
}
