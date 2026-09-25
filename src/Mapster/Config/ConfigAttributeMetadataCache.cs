using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;

namespace Mapster.Config
{
    internal sealed class ConfigAttributeMetadataCache
    {
        private readonly ConcurrentDictionary<MemberInfo, IList<CustomAttributeData>> _metadata = new(MemberInfoMetadataComparer.Instance);

        internal IEnumerable<CustomAttributeData> Get(MemberInfo member)
        {
            _metadata.TryGetValue(member, out var attributes);

            if (attributes is null)
            {
                var newAttributes = member.GetCustomAttributesData();
                _metadata.TryAdd(member, attributes);

                return attributes;
            }

            return attributes;
        }
    }


    internal sealed class MemberInfoMetadataComparer : IEqualityComparer<MemberInfo>
    {
        public static readonly MemberInfoMetadataComparer Instance = new();

        private MemberInfoMetadataComparer() { }

        public bool Equals(MemberInfo? x, MemberInfo? y)
        {
            if (x is null || y is null) return false;
            if (ReferenceEquals(x, y)) return true;
            if (x.Module != y.Module) return false;
            return x.MetadataToken == y.MetadataToken;
        }

        public int GetHashCode(MemberInfo obj)
        {
            if (obj is null) return 0;

#if NETCOREAPP2_1_OR_GREATER || NETSTANDARD2_1_OR_GREATER
            return HashCode.Combine(obj.Module, obj.MetadataToken);
#else
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + (obj.Module?.GetHashCode() ?? 0);
            hash = hash * 31 + obj.MetadataToken.GetHashCode();
            return hash;
        }
#endif
        }
    }
}
