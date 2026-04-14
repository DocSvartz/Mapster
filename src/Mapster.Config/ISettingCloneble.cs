namespace Mapster.Settings
{
    public interface ISettingCloneble<T> where T : TypeAdapterSettingsBase
    {
        public T Clone();

    }
}
