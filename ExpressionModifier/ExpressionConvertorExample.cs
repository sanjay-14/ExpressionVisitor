using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionModifier
{//exmple

    public class ExpressionConvertorExample : ExpressionVisitor
    {
        public Expression Modify(Expression expression)
        {

            return Visit(expression);
        }

        protected override Expression VisitBlock(BlockExpression node)
        {
            return base.VisitBlock(node);
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (node.NodeType == ExpressionType.AndAlso)
            {
                Expression left = this.Visit(node.Left);
                Expression right = this.Visit(node.Right);

                // Make this binary expression an OrElse operation instead of an AndAlso operation.
                return Expression.MakeBinary(ExpressionType.OrElse, left, right, node.IsLiftedToNull, node.Method);
            }
            return base.VisitBinary(node); ;
        }
    }
}
