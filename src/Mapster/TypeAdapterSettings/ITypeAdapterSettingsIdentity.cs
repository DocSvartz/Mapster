using System;

namespace Mapster
{
    public interface ITypeAdapterSettingsIdentity: IApplyable<SettingStore>
    {
        public ulong StorageVersion { get; set; }
        public ulong QuickHash { get; set; }
        public bool IsReadOnly { get; set; }
        Type? SourceType { get; set; }
        Type? DestinationType { get; set; }
        TypeAdapterSettings Clone();
        internal bool Compiled { get; set; }
    }
}
