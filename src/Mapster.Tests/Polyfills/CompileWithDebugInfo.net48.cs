#if NET48
using System;

namespace System.Linq.Expressions
{
    internal static class CompileWithDebugInfoPolyfill
    {
        public static Delegate CompileWithDebugInfo(this LambdaExpression node)
        {
            return node.Compile();
        }

        public static T CompileWithDebugInfo<T>(this Expression<T> node)
        {
            return node.Compile();
        }
    }
}
#endif
