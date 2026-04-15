using Mapster.Compile.Config;
using Mapster.Compile.Configuration.Inspectors;
using Mapster.Compile.Configuration.Matchers;
using System.Collections.Immutable;

namespace Mapster.Compile.Configuration
{
    public class TypeConverter
    {
        public TypeInspector SourceType {  get; }
        public TypeInspector TargetType { get; }
        public TypeAdapterSettingsCompile Settings {  get; }
        public TypeAdapterConfig ContextConfigs { get; }
        public ImmutableArray<MemberMatcher> MembersMapping { get; }
        public MethodInspector ConstructorMapping { get; }

    }
}
