using System;

namespace Mapster
{
    public interface ITypeAdapterSettingsIdentity: IApplyable<SettingStore>
    {
        public ulong Id { get; set; }
        public ulong Version { get; set; }
        Type? SourceType { get; set; }
        Type? DestinationType { get; set; }
        TypeAdapterSettings Clone();
        internal bool Compiled { get; set; }
    }
}
