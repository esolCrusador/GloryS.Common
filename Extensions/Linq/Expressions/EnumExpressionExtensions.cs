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
        private static readonly Type StringType = typeof (String);

        public static Expression<Func<TEnum, string>> GetEnumToStringExpression<TEnum>(Func<Enum, string> toString)
        {
            bool isNullable = false;

            Type enumType = typeof (TEnum);
            Type enumExactType = enumType;
            Type nullableEnumType = Nullable.GetUnderlyingType(enumExactType);
            if (nullableEnumType != null)
            {
                isNullable = true;
                enumExactType = nullableEnumType;
            }
            Type enumUnderlyingType = Enum.GetUnderlyingType(enumExactType);

            var allMembers = EnumExtensions.GetAllMembers(enumExactType).Select(e => new KeyValuePair<Enum, string>(e, toString(e))).OrderBy(e=>e.Value).ToList();

            ParameterExpression parameter = Expression.Parameter(enumType, enumExactType.Name.ToLower());
            Expression zeroConstant = Expression.Convert(Expression.Constant(Activator.CreateInstance(enumUnderlyingType), enumUnderlyingType), enumType);

            //Flag enum.
            if (enumExactType.GetCustomAttributes(typeof (FlagsAttribute), false).Any())
            {
                Expression<Func<string, string>> subStringExpression = enumResult => enumResult.Substring(2, int.MaxValue);

                // Ex: gender => ((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...; Result: ", Male, Female ..."
                Expression<Func<TEnum, string>> allMembersToString = allMembers.Select(kvp =>
                {
                    // (int) Gender.Male
                    Expression compareVal = Expression.Convert(Expression.Constant(kvp.Key, enumExactType), enumUnderlyingType);
                    ConstantExpression resultString = Expression.Constant(", " + kvp.Value, StringType);

                    // (gender & Gender.Male) == Gender.Male
                    Expression equalsExpression = Expression.Equal(Expression.And(Expression.Convert(parameter, enumUnderlyingType), compareVal), compareVal);
                    Expression enumToStringBody = Expression.Condition(equalsExpression, resultString, EmptyStringExpression);

                    return Expression.Lambda<Func<TEnum, string>>(enumToStringBody, parameter);
                })
                    .Join((l, r) => String.Concat(l, r));

                // Ex: gender => (((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...).Substring(2); Result: "Male, Female ..."
                Expression<Func<TEnum, string>> notNullExpression = allMembersToString.Continue(subStringExpression);

                //Zero check
                Expression checkZeroBody = Expression.Condition(Expression.Equal(parameter, zeroConstant), EmptyStringExpression, notNullExpression.Body);

                // Ex: gender => gender==0 ? "" : (((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...).Substring(2);
                notNullExpression = Expression.Lambda<Func<TEnum, string>>(checkZeroBody, notNullExpression.Parameters[0]);

                if (isNullable)
                {
                    // Ex: gender => gender==null ? null : (gender==0 ? "" : (((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...).Substring(2));
                    ConstantExpression nullEnumVal = Expression.Constant(null, enumType);
                    ConditionalExpression nullStringIfNullValue = Expression.Condition(Expression.Equal(parameter, nullEnumVal), NullStringExpression, notNullExpression.Body);

                    return Expression.Lambda<Func<TEnum, string>>(nullStringIfNullValue, parameter);
                }
                else
                {
                    // Ex: gender => gender==0 ? "" : (((gender & Gender.Male) == Gender.Male?", Male":"") + ((gender & Gender.Female) == Gender.Female?", Female":"") + ...).Substring(2);
                    return notNullExpression;
                }
            }
            else
            {
                Expression resultBody = NullStringExpression;

                foreach (var kvp in allMembers)
                {
                    ConstantExpression compareVal = Expression.Constant(kvp.Key, enumType);
                    ConstantExpression resultString = Expression.Constant(kvp.Value, StringType);

                    // gender => (gender == Gender.Male ? "Male" : (gender == Gender.Female ? "Female" : (...)));
                    resultBody = Expression.Condition(Expression.Equal(parameter, compareVal), resultString, resultBody);
                }

                //Zero check
                // gender => gender == 0 ? "" : (gender == Gender.Male ? "Male" : (gender == Gender.Female ? "Female" : (...)));
                resultBody = Expression.Condition(Expression.Equal(parameter, zeroConstant), EmptyStringExpression, resultBody);

                if (isNullable)
                {
                    ConstantExpression nullEnumVal = Expression.Constant(null, enumType);
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
