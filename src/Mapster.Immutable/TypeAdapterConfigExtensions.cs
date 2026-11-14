using Mapster.Immutable;

namespace Mapster
{
    public static class TypeAdapterConfigExtensions
    {
        public static void EnableImmutableMapping(this ITypeAdapterConfig config)
        {
            config.MappingCache.AddAdapterRule(new ImmutableAdapter());
        }
    }
}
