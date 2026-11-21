using Mapster.TypeAdapterConfigs.Builders;
using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Mapster
{
    public class FrozenTypeAdapterConfig : ITypeAdapterConfigForkable, ITypeAdapterConfigGetSnapshot
    {
        internal FrozenTypeAdapterConfig() { }

        internal FrozenTypeAdapterConfig(FrozenTypeAdapterConfigSnapshot snapshot)
        {

        }

        private ConcurrentDictionary<string, FrozenTypeAdapterConfig>? _inlineConfigs;
        private ConcurrentDictionary<string, FrozenTypeAdapterConfig> InlineConfigs =>
            _inlineConfigs ??= new ConcurrentDictionary<string, FrozenTypeAdapterConfig>();

        public ITypeAdapterConfigGetSnapshot Fork(Action<ITypeAdapterConfigSettingsProducer> action,
#if !NET40
            [CallerFilePath]
#endif
            string key1 = "",
#if !NET40
            [CallerLineNumber]
#endif
            int key2 = 0)
        {
            var key = $"{key1}|{key2}";


            return InlineConfigs.GetOrAdd(key, _ =>
            {
               var cloned = new BuilderTypeAdapterConfig();
               action(cloned);
                return new FrozenTypeAdapterConfig(new FrozenTypeAdapterConfigSnapshot(_SnapShot,cloned));
            });

        }

        private FrozenTypeAdapterConfigSnapshot? _SnapShot;

        private ConcurrentQueue<TypeAdapterConfigInitBuilder> _inits = new();

        public TypeAdapterConfigInitBuilder Init 
        {  
            get 
            {
                if(_SnapShot is null)
                {
                    var init = TypeAdapterConfigInitBuilder.Create();
                    _inits.Enqueue(init);
                    return init;
                }
                else
                {
                    throw new InvalidOperationException("FrozenTypeAdapterConfig had already been created!");
                }
            } 
        }

        public ITypeAdapterConfigSnapshot GetSnapshot => throw new NotImplementedException();
    }
}