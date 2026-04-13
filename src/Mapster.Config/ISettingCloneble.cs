namespace Mapster.Config
{
    public interface ISettingCloneble<T> where T : TypeAdapterSettingsBase
    {
        public T Clone();

    }
}
