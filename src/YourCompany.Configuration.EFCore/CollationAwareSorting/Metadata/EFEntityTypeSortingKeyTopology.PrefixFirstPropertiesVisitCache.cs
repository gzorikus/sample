using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using YourCompany.Reflection;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.Metadata
{
    public sealed partial class EFEntityTypeSortingKeyTopology
    {
        private static class PrefixFirstPropertiesVisitCache
        {
            private static readonly Expression<Action<IPrefixFirstPropertiesVisitor>>
                VisitPropertySameTypeMethod = visitor => visitor.VisitProperty<int>(null);

            private static readonly Expression<Action<IPrefixFirstPropertiesVisitor>>
                VisitPropertyConvertedMethod = visitor => visitor.VisitProperty<int, int>(null);

            private static readonly ConcurrentDictionary<
                Type, Action<IPrefixFirstPropertiesVisitor, EFEntityTypeSortingKeyTopology>>
                VisitPropertySameTypeStrategiesByPropertyType = new();

            private static readonly ConcurrentDictionary<
                (Type, Type), Action<IPrefixFirstPropertiesVisitor, EFEntityTypeSortingKeyTopology>>
                VisitPropertyConvertedStrategiesByPropertyTypes = new();

            private static readonly
                Func<Type, Action<IPrefixFirstPropertiesVisitor, EFEntityTypeSortingKeyTopology>>
                VisitPropertySameTypeStrategyCreateItemNonExclusiveStrategy = CreateVisitPropertySameTypeStrategy;

            private static readonly
                Func<(Type, Type), Action<IPrefixFirstPropertiesVisitor, EFEntityTypeSortingKeyTopology>>
                VisitPropertyConvertedStrategyCreateItemNonExclusiveStrategy = CreateVisitPropertyConvertedStrategy;

            internal static Action<IPrefixFirstPropertiesVisitor, EFEntityTypeSortingKeyTopology>
                CompileVisitPropertySameType(Type propertyValueType)
            {
                if (propertyValueType == null) throw new ArgumentNullException(nameof(propertyValueType));
                return VisitPropertySameTypeStrategiesByPropertyType
                    .GetOrAdd(propertyValueType, VisitPropertySameTypeStrategyCreateItemNonExclusiveStrategy);
            }

            internal static Action<IPrefixFirstPropertiesVisitor, EFEntityTypeSortingKeyTopology>
                CompileVisitPropertyConverted(
                    Type propertyValueType, Type providerValueType)
            {
                if (propertyValueType == null) throw new ArgumentNullException(nameof(propertyValueType));
                if (providerValueType == null) throw new ArgumentNullException(nameof(providerValueType));
                return VisitPropertyConvertedStrategiesByPropertyTypes
                    .GetOrAdd((propertyValueType, providerValueType), VisitPropertyConvertedStrategyCreateItemNonExclusiveStrategy);
            }

            private static Action<IPrefixFirstPropertiesVisitor, EFEntityTypeSortingKeyTopology>
                CreateVisitPropertySameTypeStrategy(Type propertyValueType)
            {
                var visitor = Expression.Parameter(typeof(IPrefixFirstPropertiesVisitor), "visitor");
                var topology = Expression.Parameter(typeof(EFEntityTypeSortingKeyTopology), "topology");
                var method = GetMemberHelper
                    .GetMethodInfo(VisitPropertySameTypeMethod)
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(new[] { propertyValueType });
                var call = Expression.Call(visitor, method, topology);
                return Expression
                    .Lambda<Action<IPrefixFirstPropertiesVisitor, EFEntityTypeSortingKeyTopology>>(call, visitor, topology)
                    .Compile();
            }

            private static Action<IPrefixFirstPropertiesVisitor, EFEntityTypeSortingKeyTopology>
                CreateVisitPropertyConvertedStrategy((Type PropertyValueType, Type ProviderValueType) key)
            {
                var visitor = Expression.Parameter(typeof(IPrefixFirstPropertiesVisitor), "visitor");
                var topology = Expression.Parameter(typeof(EFEntityTypeSortingKeyTopology), "topology");
                var method = GetMemberHelper
                    .GetMethodInfo(VisitPropertyConvertedMethod)
                    .GetGenericMethodDefinition()
                    .MakeGenericMethod(new[] { key.PropertyValueType, key.ProviderValueType });
                var call = Expression.Call(visitor, method, topology);
                return Expression
                    .Lambda<Action<IPrefixFirstPropertiesVisitor, EFEntityTypeSortingKeyTopology>>(call, visitor, topology)
                    .Compile();
            }
        }
    }
}