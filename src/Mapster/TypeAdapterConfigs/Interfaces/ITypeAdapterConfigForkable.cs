using System;

namespace Mapster
{
    public interface ITypeAdapterConfigForkable
    {
        ITypeAdapterConfigGetSnapshot Fork(Action<ITypeAdapterConfigSettingsProducer> action, string key1, int key2);
    }
}
