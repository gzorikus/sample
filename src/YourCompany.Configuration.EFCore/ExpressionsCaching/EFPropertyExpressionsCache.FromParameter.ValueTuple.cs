using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using YourCompany.Reflection;

namespace YourCompany.Configuration.EFCore.ExpressionsCaching
{
    public static partial class EFPropertyExpressionsCache
    {
        private static readonly ConcurrentDictionary<ValueTuplePropertyKey, MemberExpression> ValueTupleProperties = new();

        public static MemberExpression GetPreviouslyCachedValueTupleProperty(
            Expression cachedValueTupleExpression, int valueIndex)
        {
            if (cachedValueTupleExpression == null) throw new ArgumentNullException(nameof(cachedValueTupleExpression));
            if (valueIndex < 0) throw new ArgumentOutOfRangeException(nameof(valueIndex), valueIndex, message: null);
            if (!ValueTupleHelper.CheckIsValueTuple(cachedValueTupleExpression.Type))
                throw new ApplicationException("!CheckIsValueTuple(cachedValueTupleExpression.Type)");
            var key = new ValueTuplePropertyKey { CachedValueTupleExpression = cachedValueTupleExpression, ValueIndex = valueIndex };
            if (!ValueTupleProperties.TryGetValue(key, out var previouslyCachedProperyExpression))
                throw new ApplicationException("!ValueTupleProperties.TryGetValue(key, out var previouslyCachedProperyExpression)");
            return previouslyCachedProperyExpression;
        }

        public static partial class EFProperty<TProperty>
        {
            public static Expression EntitiesValueTupleParameter(
                ParameterExpression parameter, int valueIndex, string propertyName)
            {
                if (parameter == null) throw new ArgumentNullException(nameof(parameter));
                if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));

                var key = new EFPropertyKey
                {
                    CachedEntityExpression = GetPreviouslyCachedValueTupleProperty(parameter, valueIndex),
                    CachedEntityExpressionIsNullable = true,
                    PropertyName = propertyName
                };

                return EFProperties.GetOrAdd(key, CreateNonExclusiveStrategy);
            }
        }

        private record struct ValueTuplePropertyKey
        {
            internal Expression CachedValueTupleExpression { get; set; }
            internal int ValueIndex { get; set; }
        }

        public static partial class FromParameter<T>
        {
            public static class ValueTuple
            {
                public static IReadOnlyList<MemberExpression> PropertiesByValueIndex { get; }

                static ValueTuple()
                {
                    if (ValueTupleHelper.Expressions.TryGetValueTupleProperties(Instance, out var properties))
                    {
                        for (int i = 0; i < properties.Count; i++)
                        {
                            var key = new ValueTuplePropertyKey { CachedValueTupleExpression = Instance, ValueIndex = i };
                            if (!ValueTupleProperties.TryAdd(key, properties[i]))
                                throw new ApplicationException("!ValueTupleProperties.TryAdd(key, properties[i])");
                        }

                        PropertiesByValueIndex = properties;
                    }
                }
            }
        }
    }
}