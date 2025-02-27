using System.Collections.Generic;
using System.Linq;

namespace Mapster.Utils;

internal class StringArrayEqualityComparer : IEqualityComparer<string[]>
{
    public bool Equals(string[]? x, string[]? y)
    {
        if (x == null || y == null)
        {
            return x == y;
        }
        return x.SequenceEqual(y);
    }

    public int GetHashCode(string[] obj)
    {
        return obj.Aggregate(0, (hash, s) => hash ^ s.GetHashCode());
    }
}
