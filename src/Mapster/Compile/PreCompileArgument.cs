using Mapster.Models;
using System;
using System.Collections.Concurrent;

namespace Mapster
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Minor Code Smell", "S1104:Fields should not have public accessibility", Justification = "<Pending>")]
    public class PreCompileArgument
    {
        public Type SourceType;
        public Type DestinationType;
        public MapType MapType;
        public bool ExplicitMapping;
        public ConcurrentDictionary<Type, IMemberModelEx[]> TypeMemberModelsCache { get; init; }
    }
}
