using System;
using System.Linq;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public abstract partial class SortingKey : SortingKeyQueries.ISingleEntityFiltering
    {
        private SortingKeyPredicatesBuilder _predicatesBuilder;

        SortingKeyTopology.ILastProperty SortingKeyQueries.ISingleTopologyQuery.SingleTopology => this;
        SortingKeyTopology.ILastProperty SortingKeyQueries.IMultiTopologyQuery.PossibleSingleTopology => this;

        public string ReplaceQueriedSetName { get; internal set; }

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
        }

        void SortingKeyQueries.ISingleEntityEquality.EnsureCompatibleWithSingleEntityEqualityQueries(
            Type entityType, bool singleEntityReplacingQueriedSet)
            => this.EnsureCompatibleWithSingleEntityQueries(entityType, singleEntityReplacingQueriedSet);

        public IOrderedQueryable<TEntity> GetNext<TEntity>(IQueryable<TEntity> queryable) where TEntity : class
        {
            if (queryable == null) throw new ArgumentNullException(nameof(queryable));
            if (PropertyName == null) throw new ApplicationException("PropertyName == null");
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

        public IOrderedQueryable<TEntity> GetSingleEntityEqual<TEntity>(IQueryable<TEntity> queryable)
            where TEntity : class
        {
            if (queryable == null) throw new ArgumentNullException(nameof(queryable));
            if (PropertyName == null) throw new ApplicationException("PropertyName == null");
            queryable = queryable.Where(
                PredicatesBuilder.GetEqualPredicate<TEntity>(this, replacingQueriedSet: ReplaceQueriedSetName != null));
            return SingleEntitySort(queryable);
        }

        IOrderedQueryable<TEntity> SortingKeyQueries.ISingleEntitySorting.Sort<TEntity>(IQueryable<TEntity> queryable)
            where TEntity : class
            => SingleEntitySort(queryable);

        public IOrderedQueryable<TEntity> SingleEntitySort<TEntity>(IQueryable<TEntity> queryable)
            where TEntity : class
        {
            if (queryable == null) throw new ArgumentNullException(nameof(queryable));
            if (PropertyName == null) throw new ApplicationException("PropertyName == null");
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