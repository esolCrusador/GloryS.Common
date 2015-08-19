using System.Collections.Generic;
using GloryS.Common.Extensions.Linq.Expressions;

namespace System.Linq.Expressions
{
    public static class BuilderLinqExpressionExtensions
    {
        public static Expression<Func<TSource, TResult>> Continue<TSource, TItem, TResult>(this Expression<Func<TSource, TItem>> sourceExpression, Expression<Func<TItem, TResult>> continueExpression)
        {
            ExpressionVisitor rebinder;

            switch (sourceExpression.Body.NodeType)
            {
                case ExpressionType.MemberAccess:
                {
                    rebinder = new MemberRebinder(continueExpression.Parameters[0], (MemberExpression) sourceExpression.Body);
                    break;
                }
                case ExpressionType.Parameter:
                {
                    rebinder = new ParameterRebinder(continueExpression.Parameters[0], (ParameterExpression) sourceExpression.Body);
                    break;
                }
                default:
                    rebinder = new ExpressionRebinder(continueExpression.Parameters[0], sourceExpression.Body);
                    break;
            }

            Expression resultBody = rebinder.Visit(continueExpression.Body);

            return Expression.Lambda<Func<TSource, TResult>>(resultBody, sourceExpression.Parameters);
        }

        public static Expression<Func<TSource, TResult>> ReplaceParameter<TSource, TResult>(this Expression<Func<TSource, TResult>> sourceExpression, ParameterExpression parameter)
        {
            return Expression.Lambda<Func<TSource, TResult>>(ReplaceParameter(sourceExpression.Body, sourceExpression.Parameters[0], parameter), parameter);
        }

        public static Expression<Func<TNewSource, TResult>> CastParameter<TSource, TNewSource, TResult>(this Expression<Func<TSource, TResult>> sourceExpression)
            where TNewSource: TSource
        {
            ParameterExpression sourceParameter = sourceExpression.Parameters[0];
            ParameterExpression parameter = Expression.Parameter(typeof (TNewSource), sourceParameter.Name);

            return Expression.Lambda<Func<TNewSource, TResult>>(ReplaceParameter(sourceExpression.Body, sourceParameter, parameter), parameter);
        }

        public static Expression<Func<TSecondSource, TResult>> ReplaceParameter<TSource, TSecondSource, TResult>(this Expression<Func<TSource, TResult>> sourceExpression, ParameterExpression parameter)
        {
            return Expression.Lambda<Func<TSecondSource, TResult>>(ReplaceParameter(sourceExpression.Body, sourceExpression.Parameters[0], parameter), parameter);
        }


        public static Expression<Func<TSecondSource, TResult>> ReplaceParameter<TSource, TSecondSource, TResult>(this Expression<Func<TSource, TResult>> sourceExpression, Expression<Func<TSecondSource, TSource>> member)
        {
            ExpressionVisitor rebinder = new MemberRebinder(sourceExpression.Parameters[0], (MemberExpression)member.Body);

            Expression resultBody = rebinder.Visit(sourceExpression.Body);

            return Expression.Lambda<Func<TSecondSource, TResult>>(resultBody, member.Parameters[0]);
        }

        internal static Expression ReplaceParameter(this Expression sourceExpression, ParameterExpression replaceParam, ParameterExpression parameter)
        {
            ExpressionVisitor rebinder = new ParameterRebinder(replaceParam, parameter);

            return rebinder.Visit(sourceExpression);
        }

        public static Expression<Func<TSource, bool>> Not<TSource>(this Expression<Func<TSource, bool>> sourceExpression)
        {
            return Expression.Lambda<Func<TSource, bool>>(Expression.Not(sourceExpression.Body), sourceExpression.Parameters);
        }

        public static Expression<Func<TModel, bool>> EqualsExpression<TModel, TField>(this Expression<Func<TModel, TField>> memberExpression, TField value)
        {
            return Expression.Lambda<Func<TModel, bool>>(Expression.Equal(memberExpression.Body, Expression.Constant(value)), memberExpression.Parameters);
        }

        public static Expression<Func<TModel, TValue>> ConvertExpression<TModel, TField, TValue>(this Expression<Func<TModel, TField>> memberExpression)
        {
            return Expression.Lambda<Func<TModel, TValue>>(Expression.Convert(memberExpression.Body, typeof (TValue)), memberExpression.Parameters);
        }

        public static IEnumerable<Expression<Func<TSource, TResult>>> Concat<TSource, TAltSource, TResult>(this IEnumerable<Expression<Func<TSource, TResult>>> expressions, IEnumerable<Expression<Func<TAltSource, TResult>>> altSource) 
            where TSource : TAltSource
        {
            if (typeof (TAltSource) == typeof (TSource))
            {
                return Enumerable.Concat(expressions, altSource.Cast<Expression<Func<TSource, TResult>>>());
            }
            return Enumerable.Concat(expressions, altSource.Select(exp => exp.CastParameter<TAltSource, TSource, TResult>()));
        }

        public static Expression<Func<TSource, TResult>> Combine<TSource, TResult>(this IEnumerable<Expression<Func<TSource, TResult>>> expresions, Func<Expression, Expression, BinaryExpression> combineOperator)
        {
            ParameterExpression parameter = null;
            Expression resultBody = null;
            foreach (var expression in expresions)
            {
                if (resultBody == null)
                {
                    resultBody = expression.Body;
                    parameter = expression.Parameters[0];
                }
                else
                {
                    Expression expressionBody = expression.Body;

                    ParameterExpression expressionParameter = expression.Parameters[0];

                    var rebinder = new ParameterRebinder(expressionParameter, parameter);
                    expressionBody = rebinder.Visit(expressionBody);

                    resultBody = combineOperator(resultBody, expressionBody);
                }
            }

            if (resultBody == null)
            {
                throw new ArgumentException("Expressions Enumerable is empty", "expresions");
            }

            return Expression.Lambda<Func<TSource, TResult>>(resultBody, parameter);
        }

        public static Expression<Func<TSource, TResult>> Combine<TSource, TResult>(this IEnumerable<Expression<Func<TSource, TResult>>> expresions, Expression<Func<TResult, TResult, TResult>> combineExpression)
        {
            if (!(combineExpression.Body is BinaryExpression))
            {
                throw new ArgumentException("Combine Expression is not binary expression", "combineExpression");
            }

            var operatorType = combineExpression.Body.NodeType;

            Func<Expression, Expression, BinaryExpression> combineOperator =
                (left, right) =>
                    Expression.MakeBinary(operatorType, left, right);

            return Combine(expresions, combineOperator);
        }

        public static Expression<Func<TSource, TResult>> InheritInit<TSource, TResult, TBaseSource, TBaseResult>(this Expression<Func<TSource, TResult>> init, Expression<Func<TBaseSource, TBaseResult>> baseInit)
            where TSource : TBaseSource
            where TResult : TBaseResult
        {
            var rebinder = new InitializationMerge<TBaseSource, TBaseResult, TSource, TResult>(baseInit, init);

            return rebinder.ExtendInitialization();
        }

        public static Expression<Func<TSource, TResult>> InheritInit<TSource, TResult, TBaseSource, TBaseResult>(this Expression<Func<TSource, TResult>> init, Expression<Func<TSource, TBaseSource>> entityMember, Expression<Func<TBaseSource, TBaseResult>> baseInit)
            where TResult : TBaseResult
        {
            var rebinder = new IntializationFromMemberMerge<TBaseSource, TBaseResult, TSource, TResult>(baseInit, entityMember, init);

            return rebinder.ExtendInitialization();
        }

        public static Expression<Func<TSource, TResult>> AddMemberInit<TSource, TResult, TSourceMember, TMember>(this Expression<Func<TSource, TResult>> init,
            Expression<Func<TSource, TSourceMember>> sourceMember,
            Expression<Func<TResult, TMember>> member,
            Expression<Func<TSourceMember, TMember>> memberInit)
        {
            var rebinder = new MemberInitializer<TSource, TResult, TSourceMember, TMember>(init, sourceMember, member, memberInit);

            return rebinder.ExtendInitialization();
        }

        public static Expression<Func<TSource, TResult>> AddMemberInit<TSource, TResult, TSourceMember, TMember>(this Expression<Func<TSource, TResult>> init,
            Expression<Func<TSource, IEnumerable<TSourceMember>>> sourceMember,
            Expression<Func<TResult, IEnumerable<TMember>>> member,
            Expression<Func<TSourceMember, TMember>> memberInit)
        {
            var rebinder = new EnumerableInitializer<TSource, TResult, TSourceMember, TMember>(init, sourceMember, member, memberInit);

            return rebinder.ExtendInitialization();
        }

        public static Expression<Func<TSource, TResult>> AddMemberInit<TSource, TResult, TMember>(this Expression<Func<TSource, TResult>> init,
            Expression<Func<TResult, TMember>> member,
            Expression<Func<TSource, TMember>> memberInit)
        {
            var rebinder = new MemberInitializer<TSource, TResult, TSource, TMember>(init, null, member, memberInit);

            return rebinder.ExtendInitialization();
        }
    }
}
