using System;

namespace Mapster
{
    public interface ITypeAdapterConfigCustomRuleProducer
    {
        TypeAdapterSetter When(Func<PreCompileArgument, bool> canMap);
        TypeAdapterSetter When(Func<Type, Type, MapType, bool> canMap);
    }
}
