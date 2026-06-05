using System;
using System.Linq.Expressions;
using YourCompany.Configuration.EFCore.ExpressionsCaching;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    internal partial class SortingKeyPredicatesBuilder
    {
        internal bool SingleEntityReplacingQueriedSet { get; private set; }

        internal Expression<Func<TEntity, bool>> GetNextPredicate<TEntity>(
            SortingKeyQueries.IMultiTopologyQuery query, bool replacingQueriedSet)
            where TEntity : class
        {
            var body = UseForSingleEntity<TEntity>(replacingQueriedSet).GetNextExpression(query);
            var parameter = Parameter ?? throw new ApplicationException("Parameter == null");
            return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        }

        internal Expression<Func<TEntity, bool>> GetEqualPredicate<TEntity>(
            SortingKeyQueries.IMultiTopologyQuery query, bool replacingQueriedSet)
            where TEntity : class
        {
            var body = UseForSingleEntity<TEntity>(replacingQueriedSet)
                .GetAnySortingKeyEqualExpression(query, allowPartialMatch: false)
                    ?? throw new ApplicationException("body == null");
            var parameter = Parameter ?? throw new ApplicationException("Parameter == null");
            return Expression.Lambda<Func<TEntity, bool>>(body, parameter);
        }

        internal Expression<Func<TEntity, bool>> GetEqualPredicateAllowingPartialMatch<TEntity>(
            SortingKeyQueries.IMultiTopologyQuery query)
            where TEntity : class
        {
            var body = UseForSingleEntity<TEntity>(replacingQueriedSet: false)
                .GetAnySortingKeyEqualExpression(query, allowPartialMatch: true);
            var parameter = Parameter ?? throw new ApplicationException("Parameter == null");
            return body != null
                ? Expression.Lambda<Func<TEntity, bool>>(body, parameter)
                : null;
        }

        internal SortingKeyPredicatesBuilder UseForSingleEntity<TEntity>(bool replacingQueriedSet)
            where TEntity : class
        {
            if (EFPropertyExpressionsCache.FromParameter<TEntity>.ValueTuple.PropertiesByValueIndex != null)
                throw new ApplicationException("EFPropertyExpressionsCache.FromParameter<TEntity>.ValueTuple.PropertiesByValueIndex != null");

            Parameter = EFPropertyExpressionsCache.FromParameter<TEntity>.Instance;
            SingleEntityReplacingQueriedSet = replacingQueriedSet;
            MultiEntityValueTuplePropertyOwnersCount = -1;
            MultiEntityValueTuplePropertyOwnerIndecies = null;
            ResetVisitState();
            return this;
        }

        protected virtual void ValidateCurrentSortingKeyBeforeSingleEntityQueryVisit(SortingKey sortingKey)
        {
            if (Parameter == null) throw new ApplicationException("Parameter == null");
            if (ResultingPredicateCanBeNull) return;
            sortingKey.EnsureCompatibleWithSingleEntityQueries(Parameter.Type, SingleEntityReplacingQueriedSet);
        }

        protected virtual Expression GetCurrentEFPropertySingleEntity<TProperty>(string propertyName)
        {
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (Parameter == null) throw new ApplicationException("Parameter == null");
            if (CurrentSortingKeyProperty == null) throw new ApplicationException("CurrentSortingKeyProperty == null");
            if (MultiEntityValueTuplePropertyOwnerIndecies != null) throw new ApplicationException("MultiEntityValueTuplePropertyOwnerIndecies != null");
            return !ResultingPredicateCanBeNull || CurrentSortingKeyProperty.PropertyOwner.Match(Parameter.Type)
                ? EFPropertyExpressionsCache.EFProperty<TProperty>.EntityParameter(Parameter, propertyName)
                : null;
        }
    }
}