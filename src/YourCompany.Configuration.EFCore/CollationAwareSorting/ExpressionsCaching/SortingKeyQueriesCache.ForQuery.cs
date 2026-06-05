using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.Configuration.EFCore.ExpressionsCaching;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ExpressionsCaching
{
    internal static partial class SortingKeyQueriesCache
    {
        private readonly struct TopologyCacheKey : IEquatable<TopologyCacheKey>
        {
            internal SortingKeyTopology.ILastProperty SingleTopology { get; init; }
            internal string SingleEntityReplacingQueriedSetName { get; init; }

            internal IReadOnlyList<SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex>
                MultiEntityValueTuplePropertyOwnerIndecies
            { get; init; }

            public override bool Equals(object obj) => obj is TopologyCacheKey other && Equals(other);

            public bool Equals(TopologyCacheKey other)
            {
                if (!SortingKeyTopology.Comparer.Instance.Equals(SingleTopology, other.SingleTopology)) return false;
                if (SingleEntityReplacingQueriedSetName != other.SingleEntityReplacingQueriedSetName) return false;

                int? ownersCount = MultiEntityValueTuplePropertyOwnerIndecies?.Count;
                int? otherOwnersCount = other.MultiEntityValueTuplePropertyOwnerIndecies?.Count;
                if (ownersCount != otherOwnersCount) return false;

                if (!ownersCount.HasValue) return true;

                for (int i = 0; i < ownersCount.Value; i++)
                    if (!MultiEntityValueTuplePropertyOwnerIndecies[i]
                        .EqualsForQueriesCacheKeyOnly(other.MultiEntityValueTuplePropertyOwnerIndecies[i]))
                        return false;

                return true;
            }

            public override int GetHashCode()
            {
                var hash = new HashCode();
                hash.Add(SingleTopology, SortingKeyTopology.Comparer.Instance);
                hash.Add(SingleEntityReplacingQueriedSetName);

                if (MultiEntityValueTuplePropertyOwnerIndecies != null)
                {
                    hash.Add(MultiEntityValueTuplePropertyOwnerIndecies.Count);
                    for (int i = 0; i < MultiEntityValueTuplePropertyOwnerIndecies.Count; i++)
                        hash.Add(MultiEntityValueTuplePropertyOwnerIndecies[i]);
                }

                return hash.ToHashCode();
            }
        }

        internal static partial class ForQuery<T>
        {
            private static readonly ConcurrentDictionary<TopologyCacheKey, ForTopology> Topologies = new();

            private static readonly Func<TopologyCacheKey, ForTopology> CreateTopologiesItemNonExclusiveStrategy
                = cacheKey => new ForTopology(cacheKey);

            internal static Task<List<SortingKey>> GetSingleTopologySingleEntityKeysFrom(
                SortingKeyQueries.IMultiTopologySingleEntityQuery query,
                IQueryable<T> queryable,
                CancellationToken cancellationToken)
            {
                if (query == null) throw new ArgumentNullException(nameof(query));
                if (queryable == null) throw new ArgumentNullException(nameof(queryable));

                if (EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex != null)
                    throw new ApplicationException("EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex != null");

                var cacheKey = new TopologyCacheKey
                {
                    SingleTopology = query.PossibleSingleTopology ?? throw new ApplicationException("query.PossibleSingleTopology == null"),
                    SingleEntityReplacingQueriedSetName = query.ReplaceQueriedSetName
                };

                return Topologies
                    .GetOrAdd(cacheKey, CreateTopologiesItemNonExclusiveStrategy)
                    .GetKeysFrom(queryable, cancellationToken);
            }

            internal static Task<List<SortingKeyExtraValuePair<TExtraValue>>>
                GetSingleTopologySingleEntityKeysFrom<TExtraValue>(
                    SortingKeyQueries.IMultiTopologySingleEntityQuery query,
                    IQueryable<T> queryable,
                    Expression<Func<T, TExtraValue>> extraValueSelector,
                    CancellationToken cancellationToken)
            {
                if (query == null) throw new ArgumentNullException(nameof(query));
                if (queryable == null) throw new ArgumentNullException(nameof(queryable));
                if (extraValueSelector == null) throw new ArgumentNullException(nameof(extraValueSelector));

                if (EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex != null)
                    throw new ApplicationException("EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex != null");

                var cacheKey = new TopologyCacheKey
                {
                    SingleTopology = query.PossibleSingleTopology ?? throw new ApplicationException("query.PossibleSingleTopology == null"),
                    SingleEntityReplacingQueriedSetName = query.ReplaceQueriedSetName
                };


                return Topologies
                    .GetOrAdd(cacheKey, CreateTopologiesItemNonExclusiveStrategy)
                    .GetKeysFrom(queryable, extraValueSelector, cancellationToken);
            }

            internal static Task<List<SortingKey>> GetSingleTopologyMultiEntityKeysFrom(
                SortingKeyQueries.IMultiTopologyMultiEntityQuery query,
                IQueryable<T> queryable,
                CancellationToken cancellationToken)
            {
                if (query == null) throw new ArgumentNullException(nameof(query));
                if (queryable == null) throw new ArgumentNullException(nameof(queryable));

                if (EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex == null)
                    throw new ApplicationException("EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex == null");

                var cacheKey = new TopologyCacheKey
                {
                    SingleTopology = query.PossibleSingleTopology ?? throw new ApplicationException("query.PossibleSingleTopology == null"),
                    MultiEntityValueTuplePropertyOwnerIndecies
                        = query.EntitiesValueTuplePropertyOwnerIndecies
                            ?? throw new ApplicationException("query.EntitiesValueTuplePropertyOwnerIndecies == null")
                };

                return Topologies
                    .GetOrAdd(cacheKey, CreateTopologiesItemNonExclusiveStrategy)
                    .GetKeysFrom(queryable, cancellationToken);
            }

            internal static Task<List<SortingKeyExtraValuePair<TExtraValue>>>
                GetSingleTopologyMultiEntityKeysFrom<TExtraValue>(
                    SortingKeyQueries.IMultiTopologyMultiEntityQuery query,
                    IQueryable<T> queryable,
                    Expression<Func<T, TExtraValue>> extraValueSelector,
                    CancellationToken cancellationToken)
            {
                if (query == null) throw new ArgumentNullException(nameof(query));
                if (queryable == null) throw new ArgumentNullException(nameof(queryable));
                if (extraValueSelector == null) throw new ArgumentNullException(nameof(extraValueSelector));

                if (EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex == null)
                    throw new ApplicationException("EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex == null");

                var cacheKey = new TopologyCacheKey
                {
                    SingleTopology = query.PossibleSingleTopology ?? throw new ApplicationException("query.PossibleSingleTopology == null"),
                    MultiEntityValueTuplePropertyOwnerIndecies
                        = query.EntitiesValueTuplePropertyOwnerIndecies
                            ?? throw new ApplicationException("query.EntitiesValueTuplePropertyOwnerIndecies == null")
                };

                return Topologies
                    .GetOrAdd(cacheKey, CreateTopologiesItemNonExclusiveStrategy)
                    .GetKeysFrom(queryable, extraValueSelector, cancellationToken);
            }

            internal static IOrderedQueryable<T> SingleTopologyMultiEntitySort(
                SortingKeyQueries.IMultiTopologyMultiEntityQuery query, IQueryable<T> queryable)
            {
                if (query == null) throw new ArgumentNullException(nameof(query));
                if (queryable == null) throw new ArgumentNullException(nameof(queryable));

                if (EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex == null)
                    throw new ApplicationException("EFPropertyExpressionsCache.FromParameter<T>.ValueTuple.PropertiesByValueIndex == null");

                var cacheKey = new TopologyCacheKey
                {
                    SingleTopology = query.PossibleSingleTopology ?? throw new ApplicationException("query.PossibleSingleTopology == null"),
                    MultiEntityValueTuplePropertyOwnerIndecies
                        = query.EntitiesValueTuplePropertyOwnerIndecies
                            ?? throw new ApplicationException("query.EntitiesValueTuplePropertyOwnerIndecies == null")
                };

                return Topologies
                    .GetOrAdd(cacheKey, CreateTopologiesItemNonExclusiveStrategy)
                    .MultiEntitySort(queryable);
            }
        }
    }
}