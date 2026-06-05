using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.Configuration.EFCore.CollationAwareSorting.ExpressionsCaching;
using YourCompany.Reflection;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public static partial class SortingKeyQueries
    {
        public static Task<List<SortingKey>> GetKeysFrom<TEntitiesValueTuple>(
            this ISingleTopologyMultiEntityQuery query,
            IQueryable<TEntitiesValueTuple> queryable,
            CancellationToken cancellationToken)
            where TEntitiesValueTuple : struct
        {
            if (!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple)))
                throw new ApplicationException("!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple))");
            return SortingKeyQueriesCache
                .ForQuery<TEntitiesValueTuple>
                .GetSingleTopologyMultiEntityKeysFrom(query, queryable, cancellationToken);
        }

        public static Task<List<SortingKeyExtraValuePair<TExtraValue>>> GetKeysFrom<TEntitiesValueTuple, TExtraValue>(
            this ISingleTopologyMultiEntityQuery query,
            IQueryable<TEntitiesValueTuple> queryable,
            Expression<Func<TEntitiesValueTuple, TExtraValue>> extraValueSelector,
            CancellationToken cancellationToken)
            where TEntitiesValueTuple : struct
        {
            if (!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple)))
                throw new ApplicationException("!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple))");
            return SortingKeyQueriesCache
                .ForQuery<TEntitiesValueTuple>
                .GetSingleTopologyMultiEntityKeysFrom(query, queryable, extraValueSelector, cancellationToken);
        }

        public static Task<List<SortingKey>> GetSingleTopologyKeysFrom<TEntitiesValueTuple>(
            this IMultiTopologyMultiEntityQuery query,
            IQueryable<TEntitiesValueTuple> queryable,
            CancellationToken cancellationToken)
            where TEntitiesValueTuple : struct
        {
            if (!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple)))
                throw new ApplicationException("!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple))");
            return SortingKeyQueriesCache
                .ForQuery<TEntitiesValueTuple>
                .GetSingleTopologyMultiEntityKeysFrom(query, queryable, cancellationToken);
        }

        public static Task<List<SortingKeyExtraValuePair<TExtraValue>>>
            GetSingleTopologyKeysFrom<TEntitiesValueTuple, TExtraValue>(
                this IMultiTopologyMultiEntityQuery query,
                IQueryable<TEntitiesValueTuple> queryable,
                Expression<Func<TEntitiesValueTuple, TExtraValue>> extraValueSelector,
                CancellationToken cancellationToken)
                where TEntitiesValueTuple : struct
        {
            if (!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple)))
                throw new ApplicationException("!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple))");
            return SortingKeyQueriesCache
                .ForQuery<TEntitiesValueTuple>
                .GetSingleTopologyMultiEntityKeysFrom(query, queryable, extraValueSelector, cancellationToken);
        }

        public static IOrderedQueryable<TEntitiesValueTuple> GetNext<TEntitiesValueTuple>(
            this SortingKey query, IQueryable<TEntitiesValueTuple> queryable)
            where TEntitiesValueTuple : struct
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            if (!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple)))
                throw new ApplicationException("!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple))");

            var predicatesBuilder = new SortingKeyPredicatesBuilder { EnsureSingleTopology = query };
            var propertyOwnerIndecies = query.EntitiesValueTuplePropertyOwnerIndecies
                ?? throw new ApplicationException("query.EntitiesValueTuplePropertyOwnerIndecies == null");

            queryable = queryable.Where(predicatesBuilder.GetNextPredicate<TEntitiesValueTuple>(
                query, propertyOwnerIndecies));
            return SingleTopologySort(query, queryable);
        }

        public static IOrderedQueryable<TEntitiesValueTuple> GetOrderedEqual<TEntitiesValueTuple>(
            this ISingleTopologyMultiEntityQuery query, IQueryable<TEntitiesValueTuple> queryable)
            where TEntitiesValueTuple : struct
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (query.SingleTopology == null) throw new ApplicationException("query.SingleTopology == null");

            if (!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple)))
                throw new ApplicationException("!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple))");

            var predicatesBuilder = new SortingKeyPredicatesBuilder { EnsureSingleTopology = query.SingleTopology };
            var propertyOwnerIndecies = query.EntitiesValueTuplePropertyOwnerIndecies
                ?? throw new ApplicationException("query.EntitiesValueTuplePropertyOwnerIndecies == null");

            queryable = queryable.Where(predicatesBuilder.GetEqualPredicate<TEntitiesValueTuple>(
                query, propertyOwnerIndecies));
            return SingleTopologySort(query, queryable);
        }

        public static IQueryable<TEntitiesValueTuple> GetEqual<TEntitiesValueTuple>(
            this IMultiTopologyMultiEntityQuery query, IQueryable<TEntitiesValueTuple> queryable)
            where TEntitiesValueTuple : struct
        {
            if (query == null) throw new ArgumentNullException(nameof(query));

            if (!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple)))
                throw new ApplicationException("!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple))");

            var predicatesBuilder = new SortingKeyPredicatesBuilder { EnsureSingleTopology = query.PossibleSingleTopology };
            var propertyOwnerIndecies = query.EntitiesValueTuplePropertyOwnerIndecies
                ?? throw new ApplicationException("query.EntitiesValueTuplePropertyOwnerIndecies == null");

            return queryable.Where(predicatesBuilder.GetEqualPredicate<TEntitiesValueTuple>(
                query, propertyOwnerIndecies));
        }

        public static IOrderedQueryable<TEntitiesValueTuple> Sort<TEntitiesValueTuple>(
            this ISingleTopologyMultiEntityQuery query, IQueryable<TEntitiesValueTuple> queryable)
            where TEntitiesValueTuple : struct
        {
            if (!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple)))
                throw new ApplicationException("!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple))");
            return SortingKeyQueriesCache
                .ForQuery<TEntitiesValueTuple>
                .SingleTopologyMultiEntitySort(query, queryable);
        }

        public static IOrderedQueryable<TEntitiesValueTuple> SingleTopologySort<TEntitiesValueTuple>(
            this IMultiTopologyMultiEntityQuery query, IQueryable<TEntitiesValueTuple> queryable)
            where TEntitiesValueTuple : struct
        {
            if (!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple)))
                throw new ApplicationException("!ValueTupleHelper.CheckIsValueTuple(typeof(TEntitiesValueTuple))");
            return SortingKeyQueriesCache
                .ForQuery<TEntitiesValueTuple>
                .SingleTopologyMultiEntitySort(query, queryable);
        }
    }
}