using System.Reflection;

namespace Mapster.MappingModels
{
    internal class ConstructorMapping: MetodMapping
    {
        public ConstructorInfo? ConstructorInfo => this.Info as ConstructorInfo;
    }
}
