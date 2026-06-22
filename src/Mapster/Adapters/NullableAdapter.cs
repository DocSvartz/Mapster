using Mapster.Utils;
using System.Linq.Expressions;

namespace Mapster.Adapters
{
    internal class NullableAdapter : BaseAdapter
    {

        protected override int Score => 0;   //must do first

        protected override bool CanMap(PreCompileArgument arg)
        {
            return arg.SourceType.IsNullable() || arg.DestinationType.IsNullable();
        }
        protected override bool CanInline(Expression source, Expression? destination, CompileArgument arg)
        {
            return true;
        }

        protected override Expression? CreateInlineExpression(Expression source, CompileArgument arg, bool IsRequiredOnly = false)
        {
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
