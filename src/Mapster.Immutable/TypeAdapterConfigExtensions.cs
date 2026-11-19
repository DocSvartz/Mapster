using Mapster.Immutable;

namespace Mapster
{
    public static class TypeAdapterConfigExtensions
    {
        public static void EnableImmutableMapping(this ITypeAdapterConfig config)
        {
            config.AddAdapter(new ImmutableAdapter());
        }
    }
}
