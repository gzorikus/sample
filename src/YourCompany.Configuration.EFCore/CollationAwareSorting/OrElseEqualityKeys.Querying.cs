using System;
using System.Collections.Generic;
using System.Linq;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    internal sealed partial class OrElseEqualityKeys : SortingKeyQueries.ISingleEntityEquality,
        SortingKeyQueries.IMultiTopologyMultiEntityQuery
    {
        public string ReplaceQueriedSetName { get; internal set; }

        public IReadOnlyList<SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex>
            EntitiesValueTuplePropertyOwnerIndecies
        { get; set; }

        IQueryable<TEntity> SortingKeyQueries.ISingleEntityEquality.GetEqual<TEntity>(IQueryable<TEntity> queryable)
            => PossibleSingleTopology != null
                ? GetSingleTopologySingleEntityEqual(queryable)
                : GetMultiTopologySingleEntityEqual(queryable);

        internal IOrderedQueryable<TEntity> GetSingleTopologySingleEntityEqual<TEntity>(IQueryable<TEntity> queryable)
            where TEntity : class
        {
            if (queryable == null) throw new ArgumentNullException(nameof(queryable));
            if (Count == 0) throw new ApplicationException("Count == 0");
            if (PossibleSingleTopology == null) throw new ApplicationException("PossibleSingleTopology == null");
            queryable = queryable.Where(
                this[0].PredicatesBuilder.GetEqualPredicate<TEntity>(
                    this, replacingQueriedSet: ReplaceQueriedSetName != null));
            return this[0].SingleEntitySort(queryable);
        }

        internal IQueryable<TEntity> GetMultiTopologySingleEntityEqual<TEntity>(IQueryable<TEntity> queryable)
            where TEntity : class
        {
            if (queryable == null) throw new ArgumentNullException(nameof(queryable));
            if (Count < 2) throw new ApplicationException("Count < 2");
            if (PossibleSingleTopology != null) throw new ApplicationException("PossibleSingleTopology != null");
            return queryable.Where(
                this[0].PredicatesBuilder.GetEqualPredicate<TEntity>(
                    this, replacingQueriedSet: ReplaceQueriedSetName != null));
        }

        IQueryable<TEntity> SortingKeyQueries.IMultiEntityQuery.FilterOwnedPropertiesOnlyForEquality<TEntity>(
            IQueryable<TEntity> queryable)
            where TEntity : class
            => MultiEntityFilterOwnedPropertiesOnlyForEquality(queryable);

        public IQueryable<TEntity> MultiEntityFilterOwnedPropertiesOnlyForEquality<TEntity>(
            IQueryable<TEntity> queryable)
            where TEntity : class
        {
            if (queryable == null) throw new ArgumentNullException(nameof(queryable));
            if (Count == 0) throw new ApplicationException("Count == 0");
            var predicate = this[0].PredicatesBuilder.GetEqualPredicateAllowingPartialMatch<TEntity>(this);
            return predicate != null ? queryable.Where(predicate) : queryable;
        }

        void SortingKeyQueries.ISingleEntityEquality.EnsureCompatibleWithSingleEntityEqualityQueries(
            Type entityType, bool singleEntityReplacingQueriedSet)
        {
            if (Count == 0) throw new ApplicationException("Count == 0");
            for (int i = 0; i < Count; i++)
            {
                var topology = this[i] ?? throw new ApplicationException("this[i] == null");
                topology.EnsureCompatibleWithSingleEntityQueries(
                    entityType, singleEntityReplacingQueriedSet: ReplaceQueriedSetName != null);
            }
        }

        internal void EnsureCompatibleWithSingleEntityQueries()
        {
            if (Count == 0) throw new ApplicationException("Count == 0");
            var singlePropertiesOwner = this[0]?.PropertyOwner ?? throw new ApplicationException("this[0] == null");

            for (int i = 0; i < Count; i++)
            {
                var topology = this[i] ?? throw new ApplicationException("this[i] == null");
                topology.EnsureCompatibleWithSingleEntityQueries(
                    singlePropertiesOwner, singleEntityReplacingQueriedSet: ReplaceQueriedSetName != null);
            }
        }

        public int VisitComposed(SortingKeyQueries.IMultiTopologyQueryComposedSortingKeysVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            visitor.ModelProvider ??= ModelProvider ?? throw new ApplicationException("ModelProvider == null");
            if (visitor.ModelProvider != ModelProvider) throw new ApplicationException("visitor.ModelProvider != ModelProvider");
            for (int i = 0; i < Count; i++)
            {
                var sortingKey = this[i] ?? throw new ApplicationException("sortingKey == null");
                visitor.VisitSortingKey(sortingKey);
            }
            return Count;
        }
    }
}