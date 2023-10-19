using System;

namespace Mapster
{
    [Flags]
    public enum MapType
    {
        Map = 1,
        MapToTarget = 2,
        Projection = 4,
        MapToTargetPrimitive = 8,
    }
}