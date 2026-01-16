#if NET48
using System;

namespace System.Reflection
{
    internal static class TypeGetConstructorOverloadPolyfill
    {
        public static ConstructorInfo? GetConstructor(this Type type, BindingFlags bindingAttr, Type[] types)
        {
            return type.GetConstructor(bindingAttr, binder: null, types: types, modifiers: null);
        }
    }
}
#endif
