namespace Mapster.Config.Compile
{
    public class TypeAdapterSettingsCompile : TypeAdapterSettingsBase, ISettingCloneble<TypeAdapterSettingsCompile>
    {
        public TypeAdapterSettingsCompile Clone()
        {
            var settings = new TypeAdapterSettingsCompile();
            settings.Apply(this);
            return settings;
        }
    }
}
