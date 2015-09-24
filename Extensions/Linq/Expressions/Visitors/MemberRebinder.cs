using System.Linq.Expressions;

namespace GloryS.Common.Extensions.Linq.Expressions
{
    public class MemberRebinder : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        private readonly MemberExpression _member;

        public MemberRebinder(ParameterExpression parameter, MemberExpression member)
        {
            _parameter = parameter;
            _member = member;
        }

        public override Expression Visit(Expression node)
        {
            if (node == _parameter)
            {
                node = _member;
            }
            return base.Visit(node);
        }
    }
}
