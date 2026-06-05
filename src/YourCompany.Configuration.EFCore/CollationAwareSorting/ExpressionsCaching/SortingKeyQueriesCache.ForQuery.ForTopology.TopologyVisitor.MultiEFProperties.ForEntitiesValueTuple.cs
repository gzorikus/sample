using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using YourCompany.Configuration.EFCore.ExpressionsCaching;
using YourCompany.Reflection;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ExpressionsCaching
{
    internal static partial class SortingKeyQueriesCache
    {
        private static readonly Expression<Action<IQueryable<object>>> OrderByMethod
            = q => q.OrderBy<object, int>(null);

        private static readonly Expression<Action<IQueryable<object>>> OrderByDescendingMethod
            = q => q.OrderByDescending<object, int>(null);

        private static readonly Expression<Action<IOrderedQueryable<object>>> ThenByMethod
            = q => q.ThenBy<object, int>(null);

        private static readonly Expression<Action<IOrderedQueryable<object>>> ThenByDescendingMethod
            = q => q.ThenByDescending<object, int>(null);

        internal static partial class ForQuery<T>
        {
            private sealed partial class ForTopology
            {
                private abstract partial class TopologyVisitor
                {
                    internal abstract partial class MultiEFProperties
                    {
                        internal sealed class ForEntitiesValueTuple : MultiEFProperties
                        {
                            private readonly IReadOnlyList<SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex>
                                _valueTuplePropertyOwnerIndecies;

                            private readonly SortingKeyTopology.ILastProperty _matchSingleTopology;
                            private readonly bool _deduplicateEachPropertyPrefixVisit;

                            private readonly SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex[]
                                _singleTopologyValueTuplePropertyOwnerIndecies;

                            private SortingKeyTopology.ILastProperty _visitedPropertyPrefix;
                            private Expression _sortingCallChain;

                            internal ForEntitiesValueTuple(
                                SortingKeyTopology.ILastProperty singleTopology,
                                IReadOnlyList<SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex>
                                    valueTuplePropertyOwnerIndecies)
                                : base(singleTopology)
                            {
                                if (singleTopology == null) throw new ArgumentNullException(nameof(singleTopology));
                                if (valueTuplePropertyOwnerIndecies == null) throw new ArgumentNullException(nameof(valueTuplePropertyOwnerIndecies));
                                if (valueTuplePropertyOwnerIndecies.Count == 0) throw new ApplicationException("valueTuplePropertyOwnerIndecies.Count == 0");

                                if (EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex == null)
                                    throw new ApplicationException("EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex == null");

                                _valueTuplePropertyOwnerIndecies = valueTuplePropertyOwnerIndecies;
                                _matchSingleTopology = singleTopology;
                                _deduplicateEachPropertyPrefixVisit = true;
                                _singleTopologyValueTuplePropertyOwnerIndecies
                                    = new SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex[
                                        _singleTopologyPropertiesCount];

                                for (int i = 0; i < valueTuplePropertyOwnerIndecies.Count; i++)
                                {
                                    var (property, ownerIndex, _) = valueTuplePropertyOwnerIndecies[i];
                                    if (property == null) throw new ApplicationException("property == null");
                                    if (ownerIndex < 0) throw new ApplicationException("ownerIndex < 0");
                                    if (ownerIndex >= EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex.Count)
                                        throw new ApplicationException("ownerIndex >= EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex.Count");

                                    property.VisitPrefixFirst(this);
                                }

                                if (_visitedPropertyPrefix != null) throw new ApplicationException("_visitedPropertyPrefix != null");
                                if (_sortingCallChain != null) throw new ApplicationException("_sortingCallChain != null");
                                if (_visitedPropertiesTotal != valueTuplePropertyOwnerIndecies.Count)
                                    throw new ApplicationException("_visitedPropertiesTotal != valueTuplePropertyOwnerIndecies.Count");

                                BuildMultiEFPropertySelectValuesExpression();

                                _deduplicateEachPropertyPrefixVisit = false;
                                CollectCreateSortingKeyChain(singleTopology);

                                if (_sortingCallChain == null) throw new ApplicationException("_sortingCallChain == null");
                            }

                            protected override bool CheckToSKipProperty<TProperty>(
                                SortingKeyTopology.ILastProperty property)
                            {
                                if (property == null) throw new ArgumentNullException(nameof(property));
                                if (!_deduplicateEachPropertyPrefixVisit) return false;

                                int singleTopologyPropertyIndex = _visitedPropertiesTotal < _singleTopologyPropertiesCount
                                    ? _visitedPropertiesTotal
                                    : _visitedPropertiesTotal % _singleTopologyPropertiesCount;
                                if (singleTopologyPropertyIndex != property.PrefixKeysCount) return true;

                                if (!ReferenceEquals(property.Prefix, _visitedPropertyPrefix))
                                    throw new ApplicationException("!ReferenceEquals(property.Prefix, _visitedPropertyPrefix)");

                                int singleTopologyLastPropertyIndex = _singleTopologyPropertiesCount - 1;
                                if (singleTopologyPropertyIndex == singleTopologyLastPropertyIndex)
                                {
                                    if (!SortingKeyTopology.Comparer.Instance.Equals(property, _matchSingleTopology))
                                        throw new ApplicationException("!SortingKeyTopology.Comparer.Instance.Equals(property, _matchSingleTopology)");

                                    _visitedPropertyPrefix = null;
                                }
                                else
                                {
                                    _visitedPropertyPrefix = property;
                                }

                                return false;
                            }

                            internal override Func<IQueryable<T>, IOrderedQueryable<T>> CompileMultiEntitySortStrategy()
                                => Expression
                                    .Lambda<Func<IQueryable<T>, IOrderedQueryable<T>>>(
                                        _sortingCallChain ?? throw new ApplicationException("_sortingCallChain == null"),
                                        EFPropertyExpressionsCache.FromParameter<IQueryable<T>>.Instance)
                                    .Compile();

                            protected override void CollectSelectValuesExpressionVisit<TProperty>(
                                SortingKeyTopology.ILastProperty property)
                            {
                                if (property == null) throw new ArgumentNullException(nameof(property));
                                if (_selectValuesExpression != null) throw new ApplicationException("_selectValuesExpression != null");

                                if (_visitedPropertiesTotal >= _valueTuplePropertyOwnerIndecies.Count)
                                    throw new ApplicationException("_visitedPropertiesTotal >= _valueTuplePropertyOwnerIndecies.Count");

                                var (visitingProperty, ownerIndex, useForSorting) = _valueTuplePropertyOwnerIndecies[_visitedPropertiesTotal];
                                if (!ReferenceEquals(property, visitingProperty)) throw new ApplicationException("!ReferenceEquals(property, visitingProperty)");
                                if (ownerIndex < 0) throw new ApplicationException("ownerIndex < 0");
                                if (ownerIndex >= EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex.Count)
                                    throw new ApplicationException("ownerIndex >= EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex.Count");

                                if (useForSorting)
                                {
                                    if (_multiEFPropertyExpressions[property.PrefixKeysCount] != null)
                                        throw new ApplicationException("_multiEFPropertyExpressions[property.PrefixKeysCount] != null");

                                    _multiEFPropertyExpressions[property.PrefixKeysCount]
                                        = EFPropertyExpressionsCache.EFProperty<TProperty>.EntitiesValueTupleParameter(
                                            EFPropertyExpressionsCache.FromParameter<T>.Instance,
                                            ownerIndex,
                                            property.PropertyName);

                                    var singleTopologyProperty = _matchSingleTopology;

                                    while (singleTopologyProperty != null
                                        && singleTopologyProperty.PrefixKeysCount != property.PrefixKeysCount)
                                        singleTopologyProperty = singleTopologyProperty.Prefix;

                                    if (singleTopologyProperty == null)
                                        throw new ApplicationException("singleTopologyProperty.PrefixKeysCount != property.PrefixKeysCount");

                                    _singleTopologyValueTuplePropertyOwnerIndecies[property.PrefixKeysCount]
                                        = new SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex
                                        {
                                            PrefixFirstProperty = singleTopologyProperty,
                                            EntitiesValueTupleParameterValueIndex = ownerIndex,
                                            UseOwnerForSortingByThisProperty = true
                                        };
                                }
                            }

                            protected override Expression GetSelectedValue(SortingKeyTopology.ILastProperty property)
                            {
                                if (property == null) throw new ArgumentNullException(nameof(property));
                                var valuesTuplePropertyValueExpression = base.GetSelectedValue(property)
                                    ?? throw new ApplicationException("base.GetSelectedValue(property) == null");
                                var efPropertyExpression = _multiEFPropertyExpressions[property.PrefixKeysCount]
                                    ?? throw new ApplicationException("[property.PrefixKeysCount] == null");

                                MethodInfo sortingMethod;

                                if (_sortingCallChain == null)
                                {
                                    var method = property.Descending
                                        ? OrderByDescendingMethod
                                        : OrderByMethod;
                                    sortingMethod = GetMemberHelper.GetMethodInfo(method).GetGenericMethodDefinition();
                                }
                                else
                                {
                                    var method = property.Descending
                                        ? ThenByDescendingMethod
                                        : ThenByMethod;
                                    sortingMethod = GetMemberHelper.GetMethodInfo(method).GetGenericMethodDefinition();
                                }

                                sortingMethod = sortingMethod.MakeGenericMethod(new[] { typeof(T), property.PropertyValueType });

                                var valueExpressionLambda = Expression.Lambda(
                                    efPropertyExpression,
                                    EFPropertyExpressionsCache.FromParameter<T>.Instance);

                                _sortingCallChain = Expression.Call(sortingMethod,
                                    _sortingCallChain ?? EFPropertyExpressionsCache.FromParameter<IQueryable<T>>.Instance,
                                    valueExpressionLambda);

                                return valuesTuplePropertyValueExpression;
                            }

                            protected override void ConfigureCreatedSortingKeyQueryOptions(SortingKey sortingKey)
                            {
                                if (sortingKey.EntitiesValueTuplePropertyOwnerIndecies != null)
                                    throw new ApplicationException("sortingKey.EntitiesValueTuplePropertyOwnerIndecies != null");
                                sortingKey.ReplaceQueriedSetName = null;
                                sortingKey.EntitiesValueTuplePropertyOwnerIndecies
                                    = _singleTopologyValueTuplePropertyOwnerIndecies;
                            }
                        }
                    }
                }
            }
        }
    }
}