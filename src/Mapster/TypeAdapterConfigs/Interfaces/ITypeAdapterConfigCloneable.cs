using System;
using System.Runtime.CompilerServices;

namespace Mapster
{
    public interface ITypeAdapterConfigCloneable
    {
        ITypeAdapterConfig Clone();
        ITypeAdapterConfig Fork(Action<ITypeAdapterConfig> action, [CallerFilePath] string key1 = "", [CallerLineNumber] int key2 = 0);
    }
}
