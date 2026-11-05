using System;

namespace Mapster
{
    public class TypeAdapterRule
    {
        public Func<PreCompileArgument, int?> Priority { get; set; }
        public ISourceToDestinationTypeAdapterSettings Settings { get; set; }
    }
}