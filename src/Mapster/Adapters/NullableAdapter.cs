using Mapster.Models;
using Mapster.Utils;
using System.Linq.Expressions;

namespace Mapster.Adapters
{
    internal class NullableAdapter : BaseAdapter
    {

        protected override int Score => 0;   //must do first

        protected override bool CanMap(PreCompileArgument arg)
        {
            if(arg.ExplicitMapping)
                return false;
            return arg.SourceType.IsNullable() || arg.DestinationType.IsNullable();
        }
        protected override bool CanInline(Expression source, Expression? destination, CompileArgument arg)
        {
            return true;
        }

        protected override Expression? CreateInlineExpression(Expression source, CompileArgument arg, bool IsRequiredOnly = false)
        {
            if (arg.ExplicitMapping)
            {
                LambdaExpression? Convert = null;

                TypeAdapterRule? getsettings;
                arg.Context.Config.RuleMap.TryGetValue(new TypeTuple(arg.SourceType, arg.DestinationType), out getsettings);

                if (getsettings != null)
                    if (arg.MapType == MapType.MapToTarget)
                        Convert = getsettings.Settings.ConverterToTargetFactory(arg);
                    else
                        Convert = getsettings.Settings.ConverterFactory(arg);
                if (Convert != null)
                    return Convert.Apply(arg.MapType, source);
            }

            var _source = source.Type.IsNullable() 
                ? Expression.Convert(source, source.Type.GetGenericArguments()[0]) 
                : source;

            Expression adapt = CreateAdaptExpression(_source, arg.DestinationType.GetNotNullableTypeDefenition(),arg);
                       
            return adapt.ToNullableExp(arg);
        }

        protected override Expression CreateBlockExpression(Expression source, Expression destination, CompileArgument arg)
        {
            return Expression.Empty();
        }
    }
}
