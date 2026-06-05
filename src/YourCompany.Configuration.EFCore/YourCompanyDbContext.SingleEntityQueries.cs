using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourCompany.Configuration.EFCore.CollationAwareSorting;

namespace YourCompany.Configuration.EFCore
{
    public partial class YourCompanyDbContext<TConfiguration>
    {
        public Task<List<SortingKey>> GetKeysAfter<TEntity>(
            SortingKeyQueries.ISingleEntityFiltering afterKey,
            CancellationToken cancellationToken,
            int? take = null, int skip = 0, bool asNoTracking = false)
            where TEntity : class
        {
            if (afterKey == null) throw new ArgumentNullException(nameof(afterKey));
            if (take.HasValue && take.Value < 1) throw new ArgumentOutOfRangeException(nameof(take), take.Value, message: "take < 1");
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), skip, message: "skip < 0");
            IQueryable<TEntity> queryable = SubsetAfter<TEntity>(afterKey, asNoTracking);
            if (skip > 0) queryable = queryable.Skip(skip);
            if (take.HasValue) queryable = queryable.Take(take.Value);
            return afterKey.GetKeysFrom(queryable, cancellationToken);
        }

        public Task<List<SortingKeyExtraValuePair<TExtraValue>>> GetKeysAfter<TEntity, TExtraValue>(
            SortingKeyQueries.ISingleEntityFiltering afterKey,
            Expression<Func<TEntity, TExtraValue>> extraValueSelector,
            CancellationToken cancellationToken,
            int? take = null, int skip = 0, bool asNoTracking = false)
            where TEntity : class
        {
            if (afterKey == null) throw new ArgumentNullException(nameof(afterKey));
            if (extraValueSelector == null) throw new ArgumentNullException(nameof(extraValueSelector));
            if (take.HasValue && take.Value < 1) throw new ArgumentOutOfRangeException(nameof(take), take.Value, message: "take < 1");
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), skip, message: "skip < 0");
            IQueryable<TEntity> queryable = SubsetAfter<TEntity>(afterKey, asNoTracking);
            if (skip > 0) queryable = queryable.Skip(skip);
            if (take.HasValue) queryable = queryable.Take(take.Value);
            return afterKey.GetKeysFrom(queryable, extraValueSelector, cancellationToken);
        }

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

        public Task<List<SortingKey>> GetKeys<TEntity>(
            SortingKeyQueries.ISingleEntityFiltering matchingTo,
            CancellationToken cancellationToken,
            int? take = null, int skip = 0, bool asNoTracking = false)
            where TEntity : class
        {
            if (matchingTo == null) throw new ArgumentNullException(nameof(matchingTo));
            if (take.HasValue && take.Value < 1) throw new ArgumentOutOfRangeException(nameof(take), take.Value, message: "take < 1");
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), skip, message: "skip < 0");
            IQueryable<TEntity> queryable = OrderedSubset<TEntity>(matchingTo, asNoTracking);
            if (skip > 0) queryable = queryable.Skip(skip);
            if (take.HasValue) queryable = queryable.Take(take.Value);
            return matchingTo.GetKeysFrom(queryable, cancellationToken);
        }

        public Task<List<SortingKeyExtraValuePair<TExtraValue>>> GetKeys<TEntity, TExtraValue>(
            SortingKeyQueries.ISingleEntityFiltering matchingTo,
            Expression<Func<TEntity, TExtraValue>> extraValueSelector,
            CancellationToken cancellationToken,
            int? take = null, int skip = 0, bool asNoTracking = false)
            where TEntity : class
        {
            if (matchingTo == null) throw new ArgumentNullException(nameof(matchingTo));
            if (extraValueSelector == null) throw new ArgumentNullException(nameof(extraValueSelector));
            if (take.HasValue && take.Value < 1) throw new ArgumentOutOfRangeException(nameof(take), take.Value, message: "take < 1");
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), skip, message: "skip < 0");
            IQueryable<TEntity> queryable = OrderedSubset<TEntity>(matchingTo, asNoTracking);
            if (skip > 0) queryable = queryable.Skip(skip);
            if (take.HasValue) queryable = queryable.Take(take.Value);
            return matchingTo.GetKeysFrom(queryable, extraValueSelector, cancellationToken);
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

        public Task<List<SortingKey>> GetSingleTopologyKeys<TEntity>(
            SortingKeyQueries.ISingleEntityEquality matchingTo,
            CancellationToken cancellationToken,
            int? take = null, int skip = 0, bool asNoTracking = false)
            where TEntity : class
        {
            if (matchingTo == null) throw new ArgumentNullException(nameof(matchingTo));
            if (take.HasValue && take.Value < 1) throw new ArgumentOutOfRangeException(nameof(take), take.Value, message: "take < 1");
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), skip, message: "skip < 0");
            var queryable = Subset<TEntity>(matchingTo, asNoTracking);
            if (skip > 0) queryable = queryable.Skip(skip);
            if (take.HasValue) queryable = queryable.Take(take.Value);
            return matchingTo.GetSingleTopologyKeysFrom(queryable, cancellationToken);
        }

        public Task<List<SortingKeyExtraValuePair<TExtraValue>>> GetSingleTopologyKeys<TEntity, TExtraValue>(
            SortingKeyQueries.ISingleEntityEquality matchingTo,
            Expression<Func<TEntity, TExtraValue>> extraValueSelector,
            CancellationToken cancellationToken,
            int? take = null, int skip = 0, bool asNoTracking = false)
            where TEntity : class
        {
            if (matchingTo == null) throw new ArgumentNullException(nameof(matchingTo));
            if (extraValueSelector == null) throw new ArgumentNullException(nameof(extraValueSelector));
            if (take.HasValue && take.Value < 1) throw new ArgumentOutOfRangeException(nameof(take), take.Value, message: "take < 1");
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), skip, message: "skip < 0");
            var queryable = Subset<TEntity>(matchingTo, asNoTracking);
            if (skip > 0) queryable = queryable.Skip(skip);
            if (take.HasValue) queryable = queryable.Take(take.Value);
            return matchingTo.GetSingleTopologyKeysFrom(queryable, extraValueSelector, cancellationToken);
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

        public Task<List<SortingKey>> GetOrderedKeys<TEntity>(
            SortingKeyQueries.ISingleEntitySorting orderedBy,
            CancellationToken cancellationToken,
            int? take = null, int skip = 0, bool asNoTracking = false)
            where TEntity : class
        {
            if (orderedBy == null) throw new ArgumentNullException(nameof(orderedBy));
            if (take.HasValue && take.Value < 1) throw new ArgumentOutOfRangeException(nameof(take), take.Value, message: "take < 1");
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), skip, message: "skip < 0");
            IQueryable<TEntity> queryable = OrderedSet<TEntity>(orderedBy, asNoTracking);
            if (skip > 0) queryable = queryable.Skip(skip);
            if (take.HasValue) queryable = queryable.Take(take.Value);
            return orderedBy.GetKeysFrom(queryable, cancellationToken);
        }

        public Task<List<SortingKeyExtraValuePair<TExtraValue>>> GetOrderedKeys<TEntity, TExtraValue>(
            SortingKeyQueries.ISingleEntitySorting orderedBy,
            Expression<Func<TEntity, TExtraValue>> extraValueSelector,
            CancellationToken cancellationToken,
            int? take = null, int skip = 0, bool asNoTracking = false)
            where TEntity : class
        {
            if (orderedBy == null) throw new ArgumentNullException(nameof(orderedBy));
            if (extraValueSelector == null) throw new ArgumentNullException(nameof(extraValueSelector));
            if (take.HasValue && take.Value < 1) throw new ArgumentOutOfRangeException(nameof(take), take.Value, message: "take < 1");
            if (skip < 0) throw new ArgumentOutOfRangeException(nameof(skip), skip, message: "skip < 0");
            IQueryable<TEntity> queryable = OrderedSet<TEntity>(orderedBy, asNoTracking);
            if (skip > 0) queryable = queryable.Skip(skip);
            if (take.HasValue) queryable = queryable.Take(take.Value);
            return orderedBy.GetKeysFrom(queryable, extraValueSelector, cancellationToken);
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