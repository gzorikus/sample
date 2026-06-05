using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace YourCompany.Reflection
{
    public static partial class ValueTupleHelper
    {
        public static class Expressions
        {
            public static MethodCallExpression CreateValueTupleRestNormalized(params Expression[] values)
            {
                if (values == null) throw new ArgumentNullException(nameof(values));
                if (values.Length == 0) throw new ApplicationException("values.Length == 0");
                if (values.Length <= 7) return CreateValueTupleWithoutRest(values);

                var firstSeven = new Expression[7];
                Array.Copy(values, firstSeven, 7);

                var remaining = new Expression[values.Length - 7];
                Array.Copy(values, 7, remaining, 0, values.Length - 7);

                var restTuple = CreateValueTupleRestNormalized(remaining);
                return CreateValueTupleWithRest(firstSeven, restTuple);
            }

            public static MethodCallExpression CreateValueTupleWithoutRest(params Expression[] items)
                => CreateValueTupleWithoutRest((IReadOnlyList<Expression>)items);

            public static MethodCallExpression CreateValueTupleWithoutRest(IReadOnlyList<Expression> items)
            {
                if (items == null) throw new ArgumentNullException(nameof(items));
                if (items.Count == 0) throw new ApplicationException("items.Count == 0");
                if (items.Count > 7) throw new ApplicationException("items.Count > 7");
                var types = new Type[items.Count];
                for (int i = 0; i < items.Count; i++) types[i] = items[i]?.Type ?? throw new ApplicationException("items[i]?.Type == null");
                var method = typeof(ValueTuple).GetMethod(nameof(ValueTuple.Create), types);
                return Expression.Call(method, items);
            }

            public static MethodCallExpression CreateValueTupleWithRest(
                IReadOnlyList<Expression> firstSeven, Expression rest)
            {
                if (firstSeven == null) throw new ArgumentNullException(nameof(firstSeven));
                if (rest == null) throw new ArgumentNullException(nameof(rest));
                if (firstSeven.Count != 7) throw new ApplicationException("items.Count > 7");

                var types = new Type[8];
                var args = new Expression[8];
                for (int i = 0; i < 7; i++)
                {
                    var item = args[i] = firstSeven[i];
                    types[i] = item?.Type ?? throw new ApplicationException("item?.Type == null");
                }
                args[7] = rest;
                types[7] = rest?.Type ?? throw new ApplicationException("rest?.Type == null");

                var method = typeof(ValueTuple).GetMethod(nameof(ValueTuple.Create), types);
                return Expression.Call(method, args);
            }

            public static bool TryGetValueTupleProperties(
                Expression valueTupleExpression, out List<MemberExpression> properties)
            {
                if (valueTupleExpression == null) throw new ArgumentNullException(nameof(valueTupleExpression));
                properties = CheckIsValueTuple(valueTupleExpression.Type) ? new List<MemberExpression>() : null;
                if (properties != null) CollectValueTuplePropertiesRecursive(valueTupleExpression, properties);
                return properties != null;
            }

            public static void CollectValueTuplePropertiesRecursive(
                Expression valueTupleExpression, ICollection<MemberExpression> properties, bool restNormalized = false)
            {
                if (valueTupleExpression == null) throw new ArgumentNullException(nameof(valueTupleExpression));
                if (properties == null) throw new ArgumentNullException(nameof(properties));
                if (!TryGetValueTuplePropertyTypes(valueTupleExpression.Type, out var valuePropertyTypes))
                    throw new ApplicationException("!TryGetValueTuplePropertyTypes(valueTupleExpression.Type, out var valuePropertyTypes)");
                if (valuePropertyTypes.Length > 8) throw new ApplicationException("valuePropertyTypes.Length > 8");

                if (restNormalized)
                {
                    int itemsCount = Math.Min(valuePropertyTypes.Length, 7);
                    for (int i = 0; i < itemsCount; i++)
                        properties.Add(Expression.Property(valueTupleExpression, ValueTuplePropertyNames[i]));

                    if (valuePropertyTypes.Length == 8)
                    {
                        if (!CheckIsValueTuple(valuePropertyTypes[7]))
                            throw new ApplicationException("!CheckIsValueTuple(valuePropertyTypes[7])");

                        var property = Expression.Property(valueTupleExpression, ValueTuplePropertyNames[7]);
                        CollectValueTuplePropertiesRecursive(property, properties, restNormalized: true);
                    }
                }
                else
                {
                    for (int i = 0; i < valuePropertyTypes.Length; i++)
                    {
                        var property = Expression.Property(valueTupleExpression, ValueTuplePropertyNames[i]);
                        if (CheckIsValueTuple(valuePropertyTypes[i]))
                        {
                            CollectValueTuplePropertiesRecursive(property, properties, restNormalized: false);
                        }
                        else
                        {
                            properties.Add(property);
                        }
                    }
                }
            }
        }
    }
}