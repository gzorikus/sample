using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.Configuration.EFCore.CollationAwareSorting.ExpressionsCaching;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public static partial class SortingKeyQueries
    {
        public static Task<List<SortingKey>> GetKeysFrom<TEntity>(
            this ISingleTopologySingleEntityQuery query,
            IQueryable<TEntity> queryable,
            CancellationToken cancellationToken)
            where TEntity : class
            => SortingKeyQueriesCache
                .ForQuery<TEntity>
                .GetSingleTopologySingleEntityKeysFrom(query, queryable, cancellationToken);

        public static Task<List<SortingKeyExtraValuePair<TExtraValue>>> GetKeysFrom<TEntity, TExtraValue>(
            this ISingleTopologySingleEntityQuery query,
            IQueryable<TEntity> queryable,
            Expression<Func<TEntity, TExtraValue>> extraValueSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            => SortingKeyQueriesCache
                .ForQuery<TEntity>
                .GetSingleTopologySingleEntityKeysFrom(query, queryable, extraValueSelector, cancellationToken);

        public static Task<List<SortingKey>> GetSingleTopologyKeysFrom<TEntity>(
            this IMultiTopologySingleEntityQuery query,
            IQueryable<TEntity> queryable,
            CancellationToken cancellationToken)
            where TEntity : class
            => SortingKeyQueriesCache
                .ForQuery<TEntity>
                .GetSingleTopologySingleEntityKeysFrom(query, queryable, cancellationToken);

        public static Task<List<SortingKeyExtraValuePair<TExtraValue>>> GetSingleTopologyKeysFrom<TEntity, TExtraValue>(
            this IMultiTopologySingleEntityQuery query,
            IQueryable<TEntity> queryable,
            Expression<Func<TEntity, TExtraValue>> extraValueSelector,
            CancellationToken cancellationToken)
            where TEntity : class
            => SortingKeyQueriesCache
                .ForQuery<TEntity>
                .GetSingleTopologySingleEntityKeysFrom(query, queryable, extraValueSelector, cancellationToken);
    }
}