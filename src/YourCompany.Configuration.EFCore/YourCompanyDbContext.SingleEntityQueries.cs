using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using YourCompany.Configuration.EFCore.CollationAwareSorting;

namespace YourCompany.Configuration.EFCore
{
    public partial class YourCompanyDbContext<TConfiguration>
    {
        public IOrderedQueryable<TEntity> SubsetAfter<TEntity>(
            SortingKeyQueries.ISingleEntityFiltering afterKey, bool asNoTracking = false)
            where TEntity : class
        {
            if (afterKey == null) throw new ArgumentNullException(nameof(afterKey));
            if (afterKey.SingleTopology == null) throw new ApplicationException("afterKey.SingleTopology == null");
            afterKey.SingleTopology.EnsureCompatibleWithSingleEntityQueries(
                entityType: typeof(TEntity), singleEntityReplacingQueriedSet: afterKey.ReplaceQueriedSetName != null);
            return afterKey.GetNext(SingleEntityQuery<TEntity>(afterKey, asNoTracking));
        }

        public IOrderedQueryable<TEntity> OrderedSubset<TEntity>(
            SortingKeyQueries.ISingleEntityFiltering havingKey, bool asNoTracking = false)
            where TEntity : class
        {
            if (havingKey == null) throw new ArgumentNullException(nameof(havingKey));
            if (havingKey.SingleTopology == null) throw new ApplicationException("havingKey.SingleTopology == null");

            bool singleEntityReplacingQueriedSet = havingKey.ReplaceQueriedSetName != null;
            havingKey.SingleTopology.EnsureCompatibleWithSingleEntityQueries(
                entityType: typeof(TEntity), singleEntityReplacingQueriedSet);

            return havingKey.GetEqual(SingleEntityQuery<TEntity>(havingKey, asNoTracking));
        }

        public IQueryable<TEntity> Subset<TEntity>(
            SortingKeyQueries.ISingleEntityEquality havingKey, bool asNoTracking = false)
            where TEntity : class
        {
            if (havingKey == null) throw new ArgumentNullException(nameof(havingKey));

            bool singleEntityReplacingQueriedSet = havingKey.ReplaceQueriedSetName != null;
            if (havingKey.PossibleSingleTopology != null)
            {
                havingKey.PossibleSingleTopology.EnsureCompatibleWithSingleEntityQueries(
                    entityType: typeof(TEntity), singleEntityReplacingQueriedSet);
            }
            else
            {
                havingKey.EnsureCompatibleWithSingleEntityEqualityQueries(
                    entityType: typeof(TEntity), singleEntityReplacingQueriedSet);
            }

            return havingKey.GetEqual(SingleEntityQuery<TEntity>(havingKey, asNoTracking));
        }

        public IOrderedQueryable<TEntity> OrderedSet<TEntity>(
            SortingKeyQueries.ISingleEntitySorting orderedBy, bool asNoTracking = false)
            where TEntity : class
        {
            if (orderedBy == null) throw new ArgumentNullException(nameof(orderedBy));
            if (orderedBy.SingleTopology == null)
                throw new ApplicationException("orderedBy.SingleTopology == null");
            orderedBy.SingleTopology.EnsureCompatibleWithSingleEntityQueries(
                entityType: typeof(TEntity), singleEntityReplacingQueriedSet: orderedBy.ReplaceQueriedSetName != null);
            return orderedBy.Sort(SingleEntityQuery<TEntity>(orderedBy, asNoTracking));
        }

        internal IQueryable<TEntity> SingleEntityQuery<TEntity>(
            SortingKeyQueries.ISingleEntityQuery query = null, bool asNoTracking = false)
            where TEntity : class
        {
            if (query != null && query.ModelProvider != SingleRuntimeCollationAwareSortingModelProviderPerConfigurationType)
                throw new ApplicationException("query.ModelProvider != SingleRuntimeCollationAwareSortingModelProviderPerConfigurationType");
            var queryable = query?.ReplaceQueriedSetName != null
                ? base.Set<TEntity>(query.ReplaceQueriedSetName)
                : base.Set<TEntity>();
            return asNoTracking ? queryable.AsNoTracking() : queryable;
        }
    }
}