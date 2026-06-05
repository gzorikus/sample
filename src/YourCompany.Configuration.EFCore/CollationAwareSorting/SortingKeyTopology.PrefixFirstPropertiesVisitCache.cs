using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using YourCompany.Reflection;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public static partial class SortingKeyTopology
    {
        internal static class PrefixFirstPropertiesVisitCache
        {
            private static readonly Expression<Action<IPrefixFirstPropertiesVisitor>>
                VisitPropertySameTypeMethod = visitor => visitor.VisitProperty<int>(null);

            private static readonly Expression<Action<IPrefixFirstPropertiesVisitor>>
                VisitPropertyConvertedMethod = visitor => visitor.VisitProperty<int, int>(null);

            private static readonly ConcurrentDictionary<Type, Action<IPrefixFirstPropertiesVisitor, ILastProperty>>
                VisitPropertySameTypeStrategiesByPropertyType = new();

            private static readonly ConcurrentDictionary<(Type, Type), Action<IPrefixFirstPropertiesVisitor, ILastProperty>>
                VisitPropertyConvertedStrategiesByPropertyTypes = new();

            private static readonly Func<Type, Action<IPrefixFirstPropertiesVisitor, ILastProperty>>
                VisitPropertySameTypeStrategyCreateItemNonExclusiveStrategy
                    = CreateVisitPropertySameTypeStrategy;

            private static readonly Func<(Type, Type), Action<IPrefixFirstPropertiesVisitor, ILastProperty>>
                VisitPropertyConvertedStrategyCreateItemNonExclusiveStrategy
                    = CreateVisitPropertyConvertedStrategy;

            internal static Action<IPrefixFirstPropertiesVisitor, ILastProperty>
                CompileVisitPropertySameType(Type propertyValueType)
            {
                if (propertyValueType == null) throw new ArgumentNullException(nameof(propertyValueType));
                return VisitPropertySameTypeStrategiesByPropertyType
                    .GetOrAdd(propertyValueType, VisitPropertySameTypeStrategyCreateItemNonExclusiveStrategy);
            }

            internal static Action<IPrefixFirstPropertiesVisitor, ILastProperty>
                CompileVisitPropertyConverted(Type propertyValueType, Type providerValueType)
            {
                if (propertyValueType == null) throw new ArgumentNullException(nameof(propertyValueType));
                if (providerValueType == null) throw new ArgumentNullException(nameof(providerValueType));
                return VisitPropertyConvertedStrategiesByPropertyTypes
                    .GetOrAdd((propertyValueType, providerValueType), VisitPropertyConvertedStrategyCreateItemNonExclusiveStrategy);
            }

            private static Action<IPrefixFirstPropertiesVisitor, ILastProperty>
                CreateVisitPropertySameTypeStrategy(Type propertyValueType)
            {
                var visitor = Expression.Parameter(typeof(IPrefixFirstPropertiesVisitor), "visitor");
                var topology = Expression.Parameter(typeof(ILastProperty), "topology");
                var method = GetMemberHelper
                    .GetMethodInfo(VisitPropertySameTypeMethod)
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(new[] { propertyValueType });
                var call = Expression.Call(visitor, method, topology);
                return Expression
                    .Lambda<Action<IPrefixFirstPropertiesVisitor, ILastProperty>>(call, visitor, topology)
                    .Compile();
            }

            private static Action<IPrefixFirstPropertiesVisitor, ILastProperty>
                CreateVisitPropertyConvertedStrategy((Type PropertyValueType, Type ProviderValueType) key)
            {
                var visitor = Expression.Parameter(typeof(IPrefixFirstPropertiesVisitor), "visitor");
                var topology = Expression.Parameter(typeof(ILastProperty), "topology");
                var method = GetMemberHelper
                    .GetMethodInfo(VisitPropertyConvertedMethod)
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(new[] { key.PropertyValueType, key.ProviderValueType });
                var call = Expression.Call(visitor, method, topology);
                return Expression
                    .Lambda<Action<IPrefixFirstPropertiesVisitor, ILastProperty>>(call, visitor, topology)
                    .Compile();
            }
        }
    }
}