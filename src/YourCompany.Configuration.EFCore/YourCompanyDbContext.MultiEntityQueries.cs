using System;
using System.Linq;
using System.Linq.Expressions;
using YourCompany.Configuration.EFCore.CollationAwareSorting;

namespace YourCompany.Configuration.EFCore
{
    public partial class YourCompanyDbContext<TConfiguration>
    {
        public IQueryable<TEntity> FilterForMultiEntityQuery<TEntity>(
            SortingKeyQueries.IMultiEntityQuery query, string queriedSetName = null)
            where TEntity : class
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            if (query.ModelProvider != SingleRuntimeCollationAwareSortingModelProviderPerConfigurationType)
                throw new ApplicationException("query.ModelProvider != SingleRuntimeCollationAwareSortingModelProviderPerConfigurationType");
            IQueryable<TEntity> original = queriedSetName != null ? base.Set<TEntity>(queriedSetName) : base.Set<TEntity>();
            var filtered = query.FilterOwnedPropertiesOnlyForEquality(original);
            if (filtered == null || filtered == original) throw new ApplicationException("filtered == null || filtered == original");
            return filtered;
        }

        public IQueryable<(TEntityOrEntitiesValueTuple, TInner)> Join<TEntityOrEntitiesValueTuple, TInner, TKey>(
            IQueryable<TEntityOrEntitiesValueTuple> leftSide,
            Expression<Func<TEntityOrEntitiesValueTuple, TKey>> leftKeySelector,
            Expression<Func<TInner, TKey>> rightKeySelector,
            SortingKeyQueries.IMultiEntityQuery joinedSetFilter = null,
            string joinedSetName = null)
            where TInner : class
        {
            if (leftSide == null) throw new ArgumentNullException(nameof(leftSide));
            if (leftKeySelector == null) throw new ArgumentNullException(nameof(leftKeySelector));
            if (rightKeySelector == null) throw new ArgumentNullException(nameof(rightKeySelector));
            var rightSide = joinedSetFilter != null
                ? FilterForMultiEntityQuery<TInner>(joinedSetFilter, joinedSetName)
                : joinedSetName != null ? Set<TInner>(joinedSetName) : Set<TInner>();
            return leftSide.Join(rightSide, leftKeySelector, rightKeySelector, (left, right) => ValueTuple.Create(left, right));
        }

        public IQueryable<(TEntityOrEntitiesValueTuple, TInner)> LeftJoin<TEntityOrEntitiesValueTuple, TInner, TKey>(
            IQueryable<TEntityOrEntitiesValueTuple> outerSide,
            Expression<Func<TEntityOrEntitiesValueTuple, TKey>> outerKeySelector,
            Expression<Func<TInner, TKey>> innerKeySelector,
            SortingKeyQueries.IMultiEntityQuery joinedSetFilter = null,
            string joinedSetName = null)
            where TInner : class
        {
            if (outerSide == null) throw new ArgumentNullException(nameof(outerSide));
            if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
            if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
            var innerSide = joinedSetFilter != null
                ? FilterForMultiEntityQuery<TInner>(joinedSetFilter, joinedSetName)
                : joinedSetName != null ? Set<TInner>(joinedSetName) : Set<TInner>();
            return outerSide.GroupJoin(innerSide, outerKeySelector, innerKeySelector, (outer, innerSubset) => new { outer, innerSubset })
                .SelectMany(group => group.innerSubset.DefaultIfEmpty(), (group, inner) => ValueTuple.Create(group.outer, inner));
        }

        public IQueryable<(TEntityOrEntitiesValueTuple, TOuter)> RightJoin<TEntityOrEntitiesValueTuple, TOuter, TKey>(
            IQueryable<TEntityOrEntitiesValueTuple> innerSide,
            Expression<Func<TEntityOrEntitiesValueTuple, TKey>> innerKeySelector,
            Expression<Func<TOuter, TKey>> outerKeySelector,
            SortingKeyQueries.IMultiEntityQuery joinedSetFilter = null,
            string joinedSetName = null)
            where TOuter : class
        {
            if (innerSide == null) throw new ArgumentNullException(nameof(innerSide));
            if (innerKeySelector == null) throw new ArgumentNullException(nameof(innerKeySelector));
            if (outerKeySelector == null) throw new ArgumentNullException(nameof(outerKeySelector));
            var outerSide = joinedSetFilter != null
                ? FilterForMultiEntityQuery<TOuter>(joinedSetFilter, joinedSetName)
                : joinedSetName != null ? Set<TOuter>(joinedSetName) : Set<TOuter>();
            return outerSide.GroupJoin(innerSide, outerKeySelector, innerKeySelector, (outer, innerSubset) => new { outer, innerSubset })
                .SelectMany(group => group.innerSubset.DefaultIfEmpty(), (group, inner) => ValueTuple.Create(inner, group.outer));
        }

        public IQueryable<(TEntityOrEntitiesValueTuple, TRight)> CrossJoin<TEntityOrEntitiesValueTuple, TRight>(
            IQueryable<TEntityOrEntitiesValueTuple> leftSide,
            Expression<Func<(TEntityOrEntitiesValueTuple, TRight), bool>> condition,
            SortingKeyQueries.IMultiEntityQuery joinedSetFilter = null,
            string joinedSetName = null)
            where TRight : class
        {
            if (leftSide == null) throw new ArgumentNullException(nameof(leftSide));
            if (condition == null) throw new ArgumentNullException(nameof(condition));
            var rightSide = joinedSetFilter != null
                ? FilterForMultiEntityQuery<TRight>(joinedSetFilter, joinedSetName)
                : joinedSetName != null ? Set<TRight>(joinedSetName) : Set<TRight>();
            return leftSide.SelectMany(_ => rightSide, (left, right) => ValueTuple.Create(left, right)).Where(condition);
        }
    }
}