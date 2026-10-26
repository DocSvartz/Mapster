using System.Reflection;

namespace Mapster.MappingModels
{
    internal class ConstructorConveterModel: MetodConverterModel
    {
        public ConstructorInfo? ConstructorInfo => this.Info as ConstructorInfo;
    }
}
