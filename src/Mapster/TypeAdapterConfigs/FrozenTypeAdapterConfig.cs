using Mapster.TypeAdapterConfigs.Builders;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Mapster.Utils;

namespace Mapster
{
    public class FrozenTypeAdapterConfig : ITypeAdapterConfigForkable, ITypeAdapterConfigGetSnapshot
    {
        private bool _isInit = false;

        public bool IsInit => _isInit;

        internal FrozenTypeAdapterConfig() { }

        internal FrozenTypeAdapterConfig(FrozenTypeAdapterConfigSnapshot snapshot)
        {
            _isInit = true;
        }

        internal void SetSnapshot(FrozenTypeAdapterConfigSnapshot snapshot)
        {
            if(_SnapShot == null)
            {
                _SnapShot = snapshot;
                _isInit = true;
            }
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


        public ITypeAdapterConfigSnapshot GetSnapshot => _SnapShot!;

        
    }
}