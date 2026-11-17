using System;
using System.Runtime.CompilerServices;

namespace Mapster
{
    public interface ITypeAdapterConfigForkable
    {
        ITypeAdapterConfig Fork(Action<ITypeAdapterConfig> action, [CallerFilePath] string key1 = "", [CallerLineNumber] int key2 = 0);
    }
}
