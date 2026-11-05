using System;

namespace Mapster
{
    public interface ITypeAdapterSettingsIdentity: IApplyable<SettingStore>
    {
        Type? SourceType { get; set; }
        Type? DestinationType { get; set; }
        TypeAdapterSettings Clone();
        internal bool Compiled { get; set; }
    }
}
