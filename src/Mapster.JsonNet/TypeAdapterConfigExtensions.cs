using Mapster.JsonNet;

namespace Mapster
{
    public static class TypeAdapterConfigExtensions
    {
        public static void EnableJsonMapping(this TypeAdapterConfig config)
        {
            config.AddRule(new JsonAdapter().CreateRule());
        }
    }
}
