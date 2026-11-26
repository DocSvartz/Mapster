using Mapster.TypeAdapterConfigs.Builders;
using System;

namespace Mapster.TypeAdapterConfigs
{
    public static class FrozenGlobalConfig
    {
        private static object _initLock = new object();
        public static FrozenTypeAdapterConfig GlobalSettings = new FrozenTypeAdapterConfig();

        public static ITypeAdapterConfigSnapshot Snapshot = GlobalSettings.GetSnapshot;

        public static void Init (out FrozenTypeAdapterConfigSnapshot snapshot, Action<TypeAdapterConfigPatchBuilder> initAction)
        {
            lock (_initLock)
            {
                if (GlobalSettings.IsInit)
                    throw new InvalidOperationException("FrozenGlobalConfig already Init !!!. Use Fork");

                var init = TypeAdapterConfigPatchBuilder.Create();
                initAction(init);
                var result = init.Build();

                snapshot = new FrozenTypeAdapterConfigSnapshot(result.Config);
                snapshot.Compile(true);

                GlobalSettings.SetSnapshot(snapshot);

                var field = typeof(FrozenGlobalConfig).GetField(nameof(FrozenGlobalConfig.Snapshot));
                field!.SetValue(null, snapshot);
            }
        }

    }
}
