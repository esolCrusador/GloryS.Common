using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace GloryS.Common.Extensions.Linq.Expressions
{
    public abstract class InitializationRebinder<TSource, TDist>
    {
        protected readonly Expression<Func<TSource, TDist>> InitializationExpression;

        protected readonly ParameterExpression Parameter;

        protected InitializationRebinder(Expression<Func<TSource, TDist>> initializationExpression)
        {
            InitializationExpression = initializationExpression;

            Parameter = initializationExpression.Parameters[0];
        }

        public abstract Expression<Func<TSource, TDist>> ExtendInitialization(); 
    }
}
