using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using YourCompany.Reflection;

namespace YourCompany.Configuration.EFCore.ExpressionsCaching
{
    internal static partial class EFPropertyExpressionsCache
    {
        private static readonly ConcurrentDictionary<Type, ParameterExpression> Parameters = new();

        internal static ParameterExpression GetPrevioslyCachedParameter(Type parameterType)
        {
            if (parameterType == null) throw new ArgumentNullException(nameof(parameterType));
            if (!Parameters.TryGetValue(parameterType, out var previouslyCachedParameter))
                throw new ApplicationException("!Parameters.TryGetValue(parameterType, out var previouslyCachedParameter)");

            return previouslyCachedParameter;
        }

        internal static partial class EFProperty<TProperty>
        {
            internal static Expression EntityParameter(ParameterExpression parameter, string propertyName)
            {
                if (parameter == null) throw new ArgumentNullException(nameof(parameter));
                if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
                if (ValueTupleHelper.CheckIsValueTuple(parameter.Type))
                    throw new ApplicationException("ValueTupleHelper.CheckIsValueTuple(parameter.Type)");

                var key = new EFPropertyKey
                {
                    CachedEntityExpression = GetPrevioslyCachedParameter(parameter.Type),
                    CachedEntityExpressionIsNullable = false,
                    PropertyName = propertyName
                };

                return EFProperties.GetOrAdd(key, CreateNonExclusiveStrategy);
            }
        }

        internal static partial class FromParameter<T>
        {
            internal static ParameterExpression Instance { get; }

            static FromParameter()
            {
                Instance = Expression.Parameter(typeof(T), "x");
                if (!Parameters.TryAdd(typeof(T), Instance))
                    throw new ApplicationException("!Parameters.TryAdd(typeof(T), Instance)");
            }
        }
    }
}