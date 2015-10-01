using System;
using System.Linq.Expressions;

namespace GloryS.Common.Extensions.Linq.Expressions
{
    public abstract class InitRebinder<TSource, TDist>
    {
        protected readonly Expression<Func<TSource, TDist>> InitializationExpression;

        protected readonly ParameterExpression Parameter;

        protected InitRebinder(Expression<Func<TSource, TDist>> initializationExpression)
        {
            InitializationExpression = initializationExpression;

            Parameter = initializationExpression.Parameters[0];
        }

        public abstract Expression<Func<TSource, TDist>> ExtendInitialization(); 
    }
}
