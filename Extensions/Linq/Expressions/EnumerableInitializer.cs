﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace GloryS.Common.Extensions.Linq.Expressions
{
    public class EnumerableInitializer<TSource, TResult, TSourceMember, TMember>
        : InitializationRebinder<TSource, TResult>
    {
        private static readonly MethodInfo SelectMethodInfo;

        private delegate IEnumerable<TMember> EnumerableSelect(IEnumerable<TSourceMember> enumerable, Func<TSourceMember, TMember> select);

        static EnumerableInitializer ()
        {
            EnumerableSelect selectDelegate = Enumerable.Select;

            SelectMethodInfo = selectDelegate.Method;
        }

        private readonly Expression<Func<TSource, IEnumerable<TSourceMember>>> _sourceMember;
        private readonly Expression<Func<TResult, IEnumerable<TMember>>> _member;
        private readonly Expression<Func<TSourceMember, TMember>> _initialization;

        public EnumerableInitializer(Expression<Func<TSource, TResult>> initializationExpression,
                                     Expression<Func<TSource, IEnumerable<TSourceMember>>> sourceMember,
                                     Expression<Func<TResult, IEnumerable<TMember>>> member,
                                     Expression<Func<TSourceMember, TMember>> initialization)
            : base(initializationExpression)
        {
            _sourceMember = sourceMember;
            _member = member;
            _initialization = initialization;
        }

        protected virtual Expression GetInitExpression()
        {
            return Expression.Call(SelectMethodInfo, _sourceMember.Body, _initialization).ReplaceParameter(_sourceMember.Parameters[0], Parameter);
        }

        public override Expression<Func<TSource, TResult>> ExtendInitialization()
        {
            var memberInitBody = (MemberInitExpression)InitializationExpression.Body;
            List<MemberBinding> bindingsList = memberInitBody.Bindings.ToList();

            MemberInfo member = ((MemberExpression)_member.Body).Member;

            Expression memberFromSourceInit = GetInitExpression();

            var memberAssigment = (MemberAssignment)bindingsList.FirstOrDefault(m => m.Member.Name == member.Name);
            if (memberAssigment == null)
            {
                memberAssigment = Expression.Bind(member, memberFromSourceInit);
            }
            else
            {
                bindingsList.Remove(memberAssigment);
                memberAssigment = memberAssigment.Update(memberFromSourceInit);
            }
            bindingsList.Add(memberAssigment);

            return Expression.Lambda<Func<TSource, TResult>>(Expression.MemberInit(memberInitBody.NewExpression, bindingsList), Parameter); ;
        }
    }
}