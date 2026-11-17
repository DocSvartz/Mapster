using System;
using System.Runtime.CompilerServices;

namespace Mapster
{
    public class FrozenTypeAdapterConfig : ITypeAdapterConfigForkable
    {
        public ITypeAdapterConfig Fork(Action<ITypeAdapterConfig> action, [CallerFilePath] string key1 = "", [CallerLineNumber] int key2 = 0)
        {
            throw new NotImplementedException();
        }
    }
}
