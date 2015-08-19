using System;
using System.Linq.Expressions;

namespace GloryS.Common.Extensions.Linq.Expressions
{
    public class IntializationFromMemberMerge<TBaseSource, TBaseDest, TSource, TDest> : InitializationMerge<TBaseSource, TBaseDest, TSource, TDest>
        where TDest : TBaseDest
    {
        private readonly Expression<Func<TSource, TBaseSource>> _entityMember;

        public IntializationFromMemberMerge(Expression<Func<TBaseSource, TBaseDest>> baseExpr, Expression<Func<TSource, TBaseSource>> entityMember, Expression<Func<TSource, TDest>> initializationExpression)
            : base(baseExpr, initializationExpression)
        {
            _entityMember = entityMember;
        }

        protected override MemberInitExpression GetBaseInitExpressionBody()
        {
            Expression<Func<TSource, TBaseDest>> replacedInit = BaseInitExpr.ReplaceParameter(_entityMember);

            return (MemberInitExpression)replacedInit.ReplaceParameter(Parameter).Body;
        }
    }
}
