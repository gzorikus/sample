using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using YourCompany.Reflection;

namespace YourCompany.Configuration.EFCore.ExpressionsCaching
{
    internal static partial class EFPropertyExpressionsCache
    {
        internal static ConcurrentDictionary<EFPropertyKey, Expression> EFProperties { get; } = new();

        internal record struct EFPropertyKey
        {
            internal Expression CachedEntityExpression { get; set; }
            internal bool CachedEntityExpressionIsNullable { get; set; }
            internal string PropertyName { get; set; }
        }

        internal static partial class EFProperty<TProperty>
        {
            private static readonly Expression<Action> MethodExpression
                = () => EF.Property<TProperty>(null, "dummy");

            private static readonly Func<EFPropertyKey, Expression> CreateNonExclusiveStrategy = key =>
            {
                var call = Expression.Call(
                    GetMemberHelper.GetMethodInfo(MethodExpression), key.CachedEntityExpression, Expression.Constant(key.PropertyName));
                return key.CachedEntityExpressionIsNullable
                    ? Expression.Condition(Expression.Equal(key.CachedEntityExpression, Expression.Constant(null)),
                        Expression.Default(typeof(TProperty)),
                        call)
                    : call;
            };
        }
    }
}