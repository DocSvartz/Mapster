using Mapster.JsonNet;

namespace Mapster
{
    public static class TypeAdapterConfigExtensions
    {
        public static void EnableJsonMapping(this ITypeAdapterConfig config)
        {
            config.AddAdapter(new JsonAdapter());
        }
    }
}
