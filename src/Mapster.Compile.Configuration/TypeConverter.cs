using Mapster.Compile.Config;
using Mapster.Compile.Configuration.Inspectors;

namespace Mapster.Compile.Configuration
{
    internal class TypeConverter
    {
        public TypeInspector SourceType {  get; }
        public TypeInspector TargetType { get; }
        public TypeAdapterSettingsCompile Settings {  get; }
        public TypeAdapterConfig ContextConfigs { get; }
    }
}
