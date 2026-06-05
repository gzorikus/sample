using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace YourCompany.Configuration.EFCore.ExpressionsCaching
{
    public static partial class EFPropertyExpressionsCache
    {
        private static readonly ConcurrentDictionary<Type, ParameterExpression> Parameters = new();

        public static ParameterExpression GetPrevioslyCachedParameter(Type parameterType)
        {
            if (parameterType == null) throw new ArgumentNullException(nameof(parameterType));
            if (!Parameters.TryGetValue(parameterType, out var previouslyCachedParameter))
                throw new ApplicationException("!Parameters.TryGetValue(parameterType, out var previouslyCachedParameter)");

            return previouslyCachedParameter;
        }

        public static partial class EFProperty<TProperty>
        {
            public static Expression EntityParameter(ParameterExpression parameter, string propertyName)
            {
                if (parameter == null) throw new ArgumentNullException(nameof(parameter));
                if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

                var key = new EFPropertyKey
                {
                    CachedEntityExpression = GetPrevioslyCachedParameter(parameter.Type),
                    CachedEntityExpressionIsNullable = false,
                    PropertyName = propertyName
                };

                return EFProperties.GetOrAdd(key, CreateNonExclusiveStrategy);
            }
        }

        public static class FromParameter<T>
        {
            public static ParameterExpression Instance { get; }

            static FromParameter()
            {
                Instance = Expression.Parameter(typeof(T), "x");
                if (!Parameters.TryAdd(typeof(T), Instance))
                    throw new ApplicationException("!Parameters.TryAdd(typeof(T), Instance)");
            }
        }
    }
}