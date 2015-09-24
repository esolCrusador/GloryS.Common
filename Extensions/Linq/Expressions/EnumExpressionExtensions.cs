using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using GloryS.Common.Extensions;

namespace GloryS.LinqSample
{
    public static class EnumExpressionExtensions
    {
        private static readonly ConstantExpression EmptyStringExpression = Expression.Constant(String.Empty, typeof(string));
        private static readonly ConstantExpression NullStringExpression = Expression.Constant(null, typeof(string));

        public static Expression<Func<TEnum, string>> GetEnumToStringExpression<TEnum>(Func<Enum, string> toString)
        {
            bool isNullable = false;
            Type enumType = typeof (TEnum);
            Type nullableEnumType = Nullable.GetUnderlyingType(enumType);
            if (nullableEnumType != null)
            {
                isNullable = true;
                enumType = nullableEnumType;
            }

            var allMembers = EnumExtensions.GetAllMembers(enumType).Select(e => new KeyValuePair<Enum, string>(e, toString(e))).ToList();

            ParameterExpression parameter = Expression.Parameter(typeof (TEnum), enumType.Name.ToLower());

            //Flag enum.
            if (enumType.GetCustomAttributes(typeof (FlagsAttribute), false).Any())
            {
                Type enumUnderlyingType = Enum.GetUnderlyingType(enumType);
                Expression<Func<string, string>> subStringExpression = enumResult => enumResult.Substring(2, int.MaxValue);

                // Ex: gender => ((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...; Result: ", Male, Female ..."
                Expression<Func<TEnum, string>> allMembersToString = allMembers.Select(kvp =>
                {
                    Expression compareVal = Expression.Convert(Expression.Constant(kvp.Key, enumType), enumUnderlyingType);
                    ConstantExpression resultString = Expression.Constant(", " + kvp.Value, typeof (String));

                    Expression equalsExpression = Expression.Equal(Expression.And(Expression.Convert(parameter, enumUnderlyingType), compareVal), compareVal);
                    Expression enumToStringBody = Expression.Condition(equalsExpression, resultString, EmptyStringExpression);

                    return Expression.Lambda<Func<TEnum, string>>(enumToStringBody, parameter);
                })
                    .Join((l, r) => String.Concat(l, r));

                // Ex: gender => (((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...).Substring(2); Result: "Male, Female ..."
                Expression<Func<TEnum, string>> notNullExpression = allMembersToString.Continue(subStringExpression);
                if (isNullable)
                {
                    // Ex: gender => gender==null ? null : (((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...).Substring(2);
                    ConstantExpression nullEnumVal = Expression.Constant(null, typeof (TEnum));
                    ConditionalExpression nullStringIfNullValue = Expression.Condition(Expression.Equal(parameter, nullEnumVal), NullStringExpression, notNullExpression.Body);

                    return Expression.Lambda<Func<TEnum, string>>(nullStringIfNullValue, parameter);
                }
                else
                {
                    // Ex: gender => (((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...).Substring(2);
                    return notNullExpression;
                }
            }
            else
            {
                Expression resultBody = NullStringExpression;

                foreach (var kvp in allMembers)
                {
                    ConstantExpression compareVal = Expression.Constant(kvp.Key, typeof(TEnum));
                    ConstantExpression resultString = Expression.Constant(kvp.Value, typeof (String));

                    // gender => (gender == Gender.Male ? "Male" : (gender == Gender.Female ? "Female" : (...)))
                    resultBody = Expression.Condition(Expression.Equal(parameter, compareVal), resultString, resultBody);
                }

                if (isNullable)
                {
                    ConstantExpression nullEnumVal = Expression.Constant(null, typeof(TEnum));
                    ConditionalExpression nullStringIfNullValue = Expression.Condition(Expression.Equal(parameter, nullEnumVal), NullStringExpression, resultBody);

                    return Expression.Lambda<Func<TEnum, string>>(nullStringIfNullValue, parameter);
                }
                else
                {
                    return Expression.Lambda<Func<TEnum, string>>(resultBody, parameter);
                }
            }
        }
    }
}
