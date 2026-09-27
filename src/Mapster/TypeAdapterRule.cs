using System;

namespace Mapster
{
    public class TypeAdapterRule
    {
        public long Identity { get; init; } = DateTime.UtcNow.Ticks;
        public Func<PreCompileArgument, int?> Priority { get; set; }
        public TypeAdapterSettings Settings { get; set; }
        public bool IsDeleted { get; set; } = false;
    }
}