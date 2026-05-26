using System.Collections.Generic;

namespace Mapster
{
    [AdaptWith(AdaptDirectives.DestinationAsRecord)]
    public class OverrideTypesSettings : TypeAdapterSettings
    {
        public List<string> DropSettings
        {
            get => Get(nameof(DropSettings), () => new List<string>());
        }

        public override void Apply(object other)
        {
            if (other is SettingStore settingStore)
                Apply(settingStore);
        }

        public override void Apply(SettingStore other)
        {
            base.ApplyWithSkipSettings(other, DropSettings);
        }
    }
}
