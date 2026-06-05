using System;
using System.Collections.Generic;
using System.Linq;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public abstract partial class SortingKey : SortingKeyQueries.ISingleEntityFiltering,
        SortingKeyQueries.ISingleTopologyMultiEntityQuery
    {
        private SortingKeyPredicatesBuilder _predicatesBuilder;

        SortingKeyTopology.ILastProperty SortingKeyQueries.ISingleTopologyQuery.SingleTopology => this;
        SortingKeyTopology.ILastProperty SortingKeyQueries.IMultiTopologyQuery.PossibleSingleTopology => this;

        public string ReplaceQueriedSetName { get; internal set; }

        public IReadOnlyList<SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex>
            EntitiesValueTuplePropertyOwnerIndecies
        { get; internal set; }

        internal SortingKeyPredicatesBuilder PredicatesBuilder => _predicatesBuilder ??= new SortingKeyPredicatesBuilder();

        internal void EnsureNoUnexpectedQueryOptionsAfterCreation()
        {
            if (PropertyName == null) throw new ApplicationException("PropertyName == null");
            if (ReplaceQueriedSetName != null)
            {
                bool expectingQueriedSetNameReplacementForSharedEntityType
                    = PropertyOwner.IsSharedEntityType && !MultiplePropertyOwners;
                if (!expectingQueriedSetNameReplacementForSharedEntityType)
                    throw new ApplicationException("ReplaceQueriedSetName != null && !expectingQueriedSetNameReplacementForSharedEntityType");
            }

            if (EntitiesValueTuplePropertyOwnerIndecies != null) throw new ApplicationException("EntitiesValueTuplePropertyOwnerIndecies != null");
        }

        void SortingKeyQueries.ISingleEntityEquality.EnsureCompatibleWithSingleEntityEqualityQueries(
            Type entityType, bool singleEntityReplacingQueriedSet)
            => this.EnsureCompatibleWithSingleEntityQueries(entityType, singleEntityReplacingQueriedSet);

        IOrderedQueryable<TEntity> SortingKeyQueries.ISingleEntityFiltering.GetNext<TEntity>(IQueryable<TEntity> queryable)
            where TEntity : class
            => GetNext(queryable);

        internal IOrderedQueryable<TEntity> GetNext<TEntity>(IQueryable<TEntity> queryable) where TEntity : class
        {
            if (queryable == null) throw new ArgumentNullException(nameof(queryable));
            if (PropertyName == null) throw new ApplicationException("PropertyName == null");
            if (EntitiesValueTuplePropertyOwnerIndecies != null) throw new ApplicationException("EntitiesValueTuplePropertyOwnerIndecies != null");
            queryable = queryable.Where(
                PredicatesBuilder.GetNextPredicate<TEntity>(this, replacingQueriedSet: ReplaceQueriedSetName != null));
            return SingleEntitySort(queryable);
        }

        IOrderedQueryable<TEntity> SortingKeyQueries.ISingleEntityFiltering.GetEqual<TEntity>(IQueryable<TEntity> queryable)
            where TEntity : class
            => GetSingleEntityEqual(queryable);

        IQueryable<TEntity> SortingKeyQueries.ISingleEntityEquality.GetEqual<TEntity>(IQueryable<TEntity> queryable)
            where TEntity : class
            => GetSingleEntityEqual(queryable);

        internal IOrderedQueryable<TEntity> GetSingleEntityEqual<TEntity>(IQueryable<TEntity> queryable)
            where TEntity : class
        {
            if (queryable == null) throw new ArgumentNullException(nameof(queryable));
            if (PropertyName == null) throw new ApplicationException("PropertyName == null");
            if (EntitiesValueTuplePropertyOwnerIndecies != null) throw new ApplicationException("EntitiesValueTuplePropertyOwnerIndecies != null");
            queryable = queryable.Where(
                PredicatesBuilder.GetEqualPredicate<TEntity>(this, replacingQueriedSet: ReplaceQueriedSetName != null));
            return SingleEntitySort(queryable);
        }

        IQueryable<TEntity> SortingKeyQueries.IMultiEntityQuery.FilterOwnedPropertiesOnlyForEquality<TEntity>(
            IQueryable<TEntity> queryable)
            where TEntity : class
            => MultiEntityFilterOwnedPropertiesOnlyForEquality(queryable);

        internal IQueryable<TEntity> MultiEntityFilterOwnedPropertiesOnlyForEquality<TEntity>(
            IQueryable<TEntity> queryable)
            where TEntity : class
        {
            if (queryable == null) throw new ArgumentNullException(nameof(queryable));
            if (PropertyName == null) throw new ApplicationException("PropertyName == null");
            var predicate = PredicatesBuilder.GetEqualPredicateAllowingPartialMatch<TEntity>(this);
            return predicate != null ? queryable.Where(predicate) : queryable;
        }

        IOrderedQueryable<TEntity> SortingKeyQueries.ISingleEntitySorting.Sort<TEntity>(IQueryable<TEntity> queryable)
            where TEntity : class
            => SingleEntitySort(queryable);

        internal IOrderedQueryable<TEntity> SingleEntitySort<TEntity>(IQueryable<TEntity> queryable)
            where TEntity : class
        {
            if (queryable == null) throw new ArgumentNullException(nameof(queryable));
            if (PropertyName == null) throw new ApplicationException("PropertyName == null");
            if (EntitiesValueTuplePropertyOwnerIndecies != null) throw new ApplicationException("EntitiesValueTuplePropertyOwnerIndecies != null");
            return SingleEntitySortRecursive(queryable, replacingQueriedSet: ReplaceQueriedSetName != null);
        }

        protected abstract IOrderedQueryable<TEntity> SingleEntitySortRecursive<TEntity>(IQueryable<TEntity> queryable,
            bool replacingQueriedSet)
            where TEntity : class;

        int SortingKeyQueries.IMultiTopologyQuery.VisitComposed(
            SortingKeyQueries.IMultiTopologyQueryComposedSortingKeysVisitor visitor)
        {
            if (visitor == null) throw new ArgumentNullException(nameof(visitor));
            visitor.ModelProvider ??= ModelProvider;
            if (visitor.ModelProvider != ModelProvider) throw new ApplicationException("visitor.ModelProvider != ModelProvider");
            visitor.VisitSortingKey(this);
            return 1;
        }
    }
}