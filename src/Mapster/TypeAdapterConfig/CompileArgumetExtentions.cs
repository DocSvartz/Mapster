using System;
using System.Linq.Expressions;

namespace Mapster
{
    internal static class CompileArgumetExtentions
    {
        internal static LambdaExpression CreateMapExpression(this CompileArgument arg)
        {
            var fn = arg.MapType == MapType.MapToTarget
                ? arg.Settings.ConverterToTargetFactory
                : arg.Settings.ConverterFactory;
            if (fn == null)
                throw new CompileException(arg, new InvalidOperationException("ConverterFactory is not found"));
            try
            {
                return fn(arg);
            }
            catch (Exception ex)
            {
                throw new CompileException(arg, ex);
            }
        }

    }
}
