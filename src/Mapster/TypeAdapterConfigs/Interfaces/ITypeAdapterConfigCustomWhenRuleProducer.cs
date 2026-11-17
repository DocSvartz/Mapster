using System;

namespace Mapster
{
    public interface ITypeAdapterConfigCustomWhenRuleProducer
    {
        TypeAdapterSetter When(Func<PreCompileArgument, bool> canMap);
        TypeAdapterSetter When(Func<Type, Type, MapType, bool> canMap);
    }
}
