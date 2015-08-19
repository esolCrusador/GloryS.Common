using System.Linq.Expressions;

namespace GloryS.Common.Extensions.Linq.Expressions
{
    public class ParameterRebinder:ExpressionVisitor
    {
        private readonly ParameterExpression _replaceParam;
        private readonly ParameterExpression _targetParam;

        public ParameterRebinder(ParameterExpression replaceParam, ParameterExpression targetParam)
        {
            _replaceParam = replaceParam;
            _targetParam = targetParam;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (node == _replaceParam)
            {
                node = _targetParam;
            }

            return base.VisitParameter(node);
        }
    }
}
