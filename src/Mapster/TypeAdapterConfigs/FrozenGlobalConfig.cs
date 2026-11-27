using Mapster.TypeAdapterConfigs.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mapster.Utils;

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


        public static IList<IRegisterBuilder> ScanBuilders(params Assembly[] assemblies)
        {
            List<IRegisterBuilder> registers = assemblies.Select(assembly => assembly.GetLoadableTypes()
                .Where(x => typeof(IRegisterBuilder).GetTypeInfo().IsAssignableFrom(x.GetTypeInfo()) && x.GetTypeInfo().IsClass && !x.GetTypeInfo().IsAbstract))
                .SelectMany(registerTypes =>
                    registerTypes.Select(registerType => (IRegisterBuilder)Activator.CreateInstance(registerType))).ToList();

            Apply(registers);
            return registers;
        }

        public static void Apply(IEnumerable<IRegisterBuilder> registers)
        {
            lock (_initLock)
            {
                if (GlobalSettings.IsInit)
                    throw new InvalidOperationException("FrozenGlobalConfig already Init !!!. Use Fork");

                var builder = new TypeAdapterConfigInitBuilder();

                foreach (IRegisterBuilder register in registers)
                {
                    register.Register(builder);
                }

                var result = builder.Build().Config;

                var snapshot = new FrozenTypeAdapterConfigSnapshot(result);
                snapshot.Compile(true);

                if (GlobalSettings.IsInit)
                    throw new InvalidOperationException("FrozenGlobalConfig already Init !!!. Use Fork");

                GlobalSettings.SetSnapshot(snapshot);

                var field = typeof(FrozenGlobalConfig).GetField(nameof(FrozenGlobalConfig.Snapshot));
                field!.SetValue(null, snapshot);
            }
        }

    }
}
