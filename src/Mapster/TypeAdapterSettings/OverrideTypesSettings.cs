using System.Collections.Generic;

namespace Mapster
{
    [AdaptWith(AdaptDirectives.DestinationAsRecord)]
    public class OverrideTypesSettings : TypeAdapterSettings
    {
        public List<string> SkipSettings
        {
            get => Get(nameof(SkipSettings), () => new List<string>());
        }

        public bool? SkipAllSettings
        {
            get => Get(nameof(SkipAllSettings));
            set => Set(nameof(SkipAllSettings), value);
        }

        public override void Apply(object other)
        {
            if (other is SettingStore settingStore)
                Apply(settingStore);
        }

        public override void Apply(SettingStore other)
        {
            if(!SkipAllSettings.GetValueOrDefault())
                base.ApplyWithSkipSettings(other, SkipSettings);
        }
    }
}
