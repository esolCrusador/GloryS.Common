using System.Linq.Expressions;

namespace GloryS.Common.Extensions.Linq.Expressions
{
    public class ParameterChangeTypeRebinder: ExpressionVisitor
    {
        private readonly ParameterExpression _replaceParam;
        private readonly Expression _targetParam;

        public ParameterChangeTypeRebinder(ParameterExpression replaceParam, Expression targetParam)
        {
            _replaceParam = replaceParam;
            _targetParam = targetParam;
        }

        public override Expression Visit(Expression node)
        {
            if (node == _replaceParam)
            {
                node = _targetParam;
            }

            return base.Visit(node);
        }
    }
}
