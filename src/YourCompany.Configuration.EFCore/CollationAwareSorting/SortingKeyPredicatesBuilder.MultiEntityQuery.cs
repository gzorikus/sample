using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using YourCompany.Configuration.EFCore.ExpressionsCaching;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    internal partial class SortingKeyPredicatesBuilder : SortingKeyPropertiesVisitor
    {
        internal int MultiEntityValueTuplePropertyOwnersCount { get; private set; } = -1;

        internal IReadOnlyList<SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex>
            MultiEntityValueTuplePropertyOwnerIndecies
        { get; private set; }

        internal Expression<Func<TEntitiesValueTuple, bool>> GetNextPredicate<TEntitiesValueTuple>(
            SortingKeyQueries.IMultiTopologyQuery query,
            IReadOnlyList<SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex> propertyOwnerIndecies)
            where TEntitiesValueTuple : struct
        {
            var body = UseForMultiEntity<TEntitiesValueTuple>(propertyOwnerIndecies).GetNextExpression(query);
            var parameter = Parameter ?? throw new ApplicationException("Parameter == null");
            return Expression.Lambda<Func<TEntitiesValueTuple, bool>>(body, parameter);
        }

        internal Expression<Func<TEntitiesValueTuple, bool>> GetEqualPredicate<TEntitiesValueTuple>(
            SortingKeyQueries.IMultiTopologyQuery query,
            IReadOnlyList<SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex> propertyOwnerIndecies,
            bool allowPartialMatch = false)
            where TEntitiesValueTuple : struct
        {
            var body = UseForMultiEntity<TEntitiesValueTuple>(propertyOwnerIndecies)
                .GetAnySortingKeyEqualExpression(query, allowPartialMatch);
            var parameter = Parameter ?? throw new ApplicationException("Parameter == null");
            return Expression.Lambda<Func<TEntitiesValueTuple, bool>>(body, parameter);
        }

        internal SortingKeyPredicatesBuilder UseForMultiEntity<TEntitiesValueTuple>(
            IReadOnlyList<SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex> propertyOwnerIndecies)
            where TEntitiesValueTuple : struct
        {
            if (propertyOwnerIndecies == null) throw new ArgumentNullException(nameof(propertyOwnerIndecies));
            int ownersCount = EFPropertyExpressionsCache.FromParameter<TEntitiesValueTuple>.ValueTuple.PropertiesByValueIndex?.Count
                ?? throw new ApplicationException("EFPropertyExpressionsCache.FromParameter<TEntitiesValueTuple>.ValueTuple.PropertiesByValueIndex == null");

            Parameter = EFPropertyExpressionsCache.FromParameter<TEntitiesValueTuple>.Instance;
            MultiEntityValueTuplePropertyOwnersCount = ownersCount;
            MultiEntityValueTuplePropertyOwnerIndecies = propertyOwnerIndecies;
            SingleEntityReplacingQueriedSet = false;
            ResetVisitState();
            return this;
        }

        protected virtual Expression GetCurrentEFPropertyMultiEntity<TProperty>(string propertyName)
        {
            if (CurrentSortingKeyProperty == null) throw new ApplicationException("CurrentSortingKeyProperty == null");
            if (MultiEntityValueTuplePropertyOwnerIndecies == null) throw new ApplicationException("MultiEntityValueTuplePropertyOwnerIndecies != null");

            if (VisitedPropertiesTotal >= MultiEntityValueTuplePropertyOwnerIndecies.Count)
                throw new ApplicationException("VisitedPropertiesTotal >= MultiEntityValueTuplePropertyOwnerIndecies.Count");
            var (ownerIndexPrefixFirstProperty, ownerIndex, _) = MultiEntityValueTuplePropertyOwnerIndecies[VisitedPropertiesTotal];
            if (ownerIndexPrefixFirstProperty != null
                && !SortingKeyTopology.Comparer.Instance.Equals(ownerIndexPrefixFirstProperty, CurrentSortingKeyProperty))
                throw new ApplicationException("!SortingKeyTopology.Comparer.Instance.Equals(ownerIndexPrefixFirstProperty, CurrentSortingKeyProperty)");

            bool validOwnerIndex = ownerIndexPrefixFirstProperty != null
                && ownerIndex >= 0
                && ownerIndex < MultiEntityValueTuplePropertyOwnersCount;

            if (!ResultingPredicateCanBeNull && !validOwnerIndex) throw new ApplicationException("!ResultingPredicateCanBeNull && !validOwnerIndex");
            return !ResultingPredicateCanBeNull || validOwnerIndex
                ? EFPropertyExpressionsCache.EFProperty<TProperty>.EntitiesValueTupleParameter(Parameter, ownerIndex, propertyName)
                : null;
        }
    }
}