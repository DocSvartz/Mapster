using System.Collections.Generic;
using System.Linq.Expressions;

namespace Mapster.Utils
{
    public sealed class TopLevelMemberNameVisitor : ExpressionVisitor
    {
        public string? MemberName { get; private set; }

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;
            switch (node.NodeType)
            {
                case ExpressionType.MemberAccess:
                    {
                        if (string.IsNullOrEmpty(MemberName))
                            MemberName = ((MemberExpression)node).Member.Name;

                        return base.Visit(node);
                    }
            }

            return base.Visit(node);
        }
    }

    public sealed class QuoteVisitor : ExpressionVisitor
    {
        public List<UnaryExpression> Quotes { get; private set; } = new();

        public override Expression Visit(Expression node)
        {
            if (node == null)
                return null;
            switch (node.NodeType)
            {
                case ExpressionType.Quote:
                    {
                        Quotes.Add((UnaryExpression)node);
                        return base.Visit(node);
                    }
            }

            return base.Visit(node);
        }
    }
}
