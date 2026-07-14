using System;
using System.Linq.Expressions;

public class NullCheckFinder : ExpressionVisitor
{
    private readonly ParameterExpression _targetParameter;
    public bool FoundNullCheck { get; private set; }

    public NullCheckFinder(ParameterExpression targetParameter)
    {
        _targetParameter = targetParameter ?? throw new ArgumentNullException(nameof(targetParameter));
        FoundNullCheck = false;
    }

    protected override Expression VisitBinary(BinaryExpression node)
    {
       
        if (node.NodeType == ExpressionType.Equal || node.NodeType == ExpressionType.NotEqual)
        {

            var isLeftTarget = IsSameParameter(node.Left);
            var isRightTarget = IsSameParameter(node.Right);

           
            var otherSideIsNull = 
                (!isLeftTarget && IsNullConstant(node.Left)) || 
                (!isRightTarget && IsNullConstant(node.Right));

            if ((isLeftTarget || isRightTarget) && otherSideIsNull)
            {
                FoundNullCheck = true;
             
                return node; 
            }
        }
        
        return base.VisitBinary(node);
    }

    private bool IsSameParameter(Expression exp)
    {
        return exp is ParameterExpression param && param == _targetParameter;
    }

    private static bool IsNullConstant(Expression exp)
    {
        return exp is ConstantExpression c && c.Value == null;
    }
}