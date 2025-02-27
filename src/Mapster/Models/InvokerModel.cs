using System.Linq.Expressions;
using Mapster.Utils;

namespace Mapster.Models
{
    public class InvokerModel
    {
        public string[] DestinationMemberPath { get; set; }
        public LambdaExpression? Invoker { get; set; }
        public string[]? SourceMemberPath { get; set; }
        public LambdaExpression? Condition { get; set; }
        public bool IsChildPath { get; set; }

        public InvokerModel? Next(ParameterExpression source, string destMemberName)
        {
            if (DestinationMemberPath.Length == 0
                || DestinationMemberPath[0] != destMemberName)
                return null;

            return new InvokerModel
            {
                DestinationMemberPath = DestinationMemberPath[1..],
                Condition = IsChildPath || Condition == null
                    ? Condition
                    : Expression.Lambda(Condition.Apply(source), source),
                Invoker = IsChildPath
                    ? Invoker
                    : Expression.Lambda(GetInvokingExpression(source), source),
                SourceMemberPath = SourceMemberPath,
                IsChildPath = true,
            };
        }

        public Expression GetInvokingExpression(Expression exp, MapType mapType = MapType.Map)
        {
            if (IsChildPath)
                return Invoker!.Body;
            return SourceMemberPath != null
                ? ExpressionEx.PropertyOrFieldPath(exp, SourceMemberPath)
                : Invoker!.Apply(mapType, exp);
        }

        public Expression? GetConditionExpression(Expression exp, MapType mapType = MapType.Map)
        {
            return IsChildPath
                ? Condition?.Body
                : Condition?.Apply(mapType, exp);
        }
    }
}