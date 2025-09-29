using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

public static class MemberExpressionExtractor
{
    public static IEnumerable<MemberInfo> GetAllMemberExpressionsMemberInfo(this Expression expression)
    {
        var result = new List<MemberInfo>();
        CollectMemberInfos(expression, result);
        return result;
    }

    private static void CollectMemberInfos(Expression expression, ICollection<MemberInfo> results)
    {
        if (expression == null) return;

        if (expression is MemberExpression memberExpression)
        {
            results.Add(memberExpression.Member);
        }
        else if (expression is BinaryExpression binaryExpression && binaryExpression.NodeType == ExpressionType.Assign)
        {
            ProcessBinaryAssign(binaryExpression, results);
        }

        foreach (var subExpression in expression.GetSubExpressions())
        {
            CollectMemberInfos(subExpression, results);
        }
    }

    private static void ProcessBinaryAssign(BinaryExpression assignExpression, ICollection<MemberInfo> results)
    {
        if (assignExpression == null) return;

        if (assignExpression.Left is MemberExpression leftMember)
        {
            results.Add(leftMember.Member);
        }

        CollectMemberInfos(assignExpression.Right, results);
    }

    private static IEnumerable<Expression> GetSubExpressions(this Expression expression)
    {
        if (expression == null) yield break;

        switch (expression.NodeType)
        {
            case ExpressionType.MemberAccess:
                yield return ((MemberExpression)expression).Expression;
                break;

            case ExpressionType.Call:
                foreach (var arg in ((MethodCallExpression)expression).Arguments)
                    yield return arg;
                yield return ((MethodCallExpression)expression).Object;
                break;

            case ExpressionType.Lambda:
                yield return ((LambdaExpression)expression).Body;
                break;

            case ExpressionType.Block:
                foreach (var blockExpr in ((BlockExpression)expression).Expressions)
                    yield return blockExpr;
                break;

            case ExpressionType.Conditional:
                yield return ((ConditionalExpression)expression).Test;
                yield return ((ConditionalExpression)expression).IfTrue;
                yield return ((ConditionalExpression)expression).IfFalse;
                break;

            case ExpressionType.NewArrayInit or ExpressionType.NewArrayBounds:
                foreach (var arrayItem in ((NewArrayExpression)expression).Expressions)
                    yield return arrayItem;
                break;

            case ExpressionType.New:
                foreach (var arg in ((NewExpression)expression).Arguments)
                    yield return arg;
                break;

            case ExpressionType.Invoke:
                yield return ((InvocationExpression)expression).Expression;
                break;

            case ExpressionType.Assign:
                yield return ((BinaryExpression)expression).Left;
                yield return ((BinaryExpression)expression).Right;
                break;

            case ExpressionType.Add:
            case ExpressionType.Subtract:
            case ExpressionType.Multiply:
            case ExpressionType.Divide:
            case ExpressionType.Equal:
            case ExpressionType.NotEqual:
            case ExpressionType.GreaterThan:
            case ExpressionType.LessThan:
            case ExpressionType.AndAlso:
            case ExpressionType.OrElse:
                yield return ((BinaryExpression)expression).Left;
                yield return ((BinaryExpression)expression).Right;
                break;

            case ExpressionType.Not:
            case ExpressionType.Negate:
            case ExpressionType.Convert:
            case ExpressionType.Increment:
            case ExpressionType.Decrement:
            case ExpressionType.Quote:
            case ExpressionType.TypeAs:
            case ExpressionType.OnesComplement:
                yield return ((UnaryExpression)expression).Operand;
                break;

            case ExpressionType.TypeIs:
                yield return ((TypeBinaryExpression)expression).Expression;
                break;

            case ExpressionType.Coalesce:
                yield return ((BinaryExpression)expression).Left;
                yield return ((BinaryExpression)expression).Conversion;
                yield return ((BinaryExpression)expression).Right;
                break;

            case ExpressionType.Index:
                yield return ((IndexExpression)expression).Object;
                foreach (var indexArg in ((IndexExpression)expression).Arguments)
                    yield return indexArg;
                break;

            case ExpressionType.Loop:
                yield return ((LoopExpression)expression).Body;
                break;

            case ExpressionType.Try:
                yield return ((TryExpression)expression).Body;
                foreach (var handler in ((TryExpression)expression).Handlers)
                    yield return handler.Body;
                yield return ((TryExpression)expression).Finally;
                break;


            default:
                break;
        }
    }
}