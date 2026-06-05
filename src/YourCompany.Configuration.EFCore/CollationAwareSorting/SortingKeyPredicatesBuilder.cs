using System;
using System.Linq.Expressions;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    internal partial class SortingKeyPredicatesBuilder : SortingKeyPropertiesVisitor
    {
        internal ParameterExpression Parameter { get; private set; }
        internal PredicateKind BuildingPredicateKind { get; private set; }
        internal BinaryExpression CurrentSortingKeyPredecessorEqualExpression { get; private set; }
        internal BinaryExpression CurrentSortingKeyPropertyPrefixNextExpression { get; private set; }
        internal BinaryExpression CurrentSortingKeyPropertyPrefixEqualExpression { get; private set; }
        internal bool ResultingPredicateCanBeNull { get; private set; }

        internal BinaryExpression GetNextExpression(SortingKeyQueries.IMultiTopologyQuery query)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            ResetVisitState();
            BuildingPredicateKind = PredicateKind.NextBySingleSortingKey;
            ResultingPredicateCanBeNull = false;
            int sortingKeysCount = query.VisitComposed(this);
            if (sortingKeysCount != 1) throw new ApplicationException("sortingKeysCount != 1");
            return CurrentSortingKeyPropertyPrefixNextExpression ?? throw new ApplicationException("CurrentSortingKeyPropertyPrefixNextExpression == null");
        }

        internal BinaryExpression GetAnySortingKeyEqualExpression(
            SortingKeyQueries.IMultiTopologyQuery query, bool allowPartialMatch = false)
        {
            if (query == null) throw new ArgumentNullException(nameof(query));
            ResetVisitState();
            BuildingPredicateKind = PredicateKind.EqualByAnySortingKey;
            ResultingPredicateCanBeNull = allowPartialMatch;
            int sortingKeysCount = query.VisitComposed(this);
            if (!allowPartialMatch && sortingKeysCount < 1) throw new ApplicationException("!allowPartialMatch && sortingKeysCount < 1");
            return allowPartialMatch
                ? CurrentSortingKeyPredecessorEqualExpression
                : CurrentSortingKeyPredecessorEqualExpression
                    ?? throw new ApplicationException("CurrentSortingKeyPredecessorEqualExpression == null");
        }

        protected override void VisitCurrentProperty<TProperty>(SortingKey property, TProperty value)
        {
            base.VisitCurrentProperty(property, value);
            switch (BuildingPredicateKind)
            {
                case PredicateKind.Unspecified: return;
                case PredicateKind.NextBySingleSortingKey: BuildCurrentPropertyNext<TProperty>(); break;
                case PredicateKind.EqualByAnySortingKey: BuildCurrentPropertyEqual<TProperty>(); break;
                default: throw new ApplicationException(nameof(BuildingPredicateKind));
            }
        }

        protected override void ValidateCurrentSortingKeyBeforeVisit(SortingKey sortingKey)
        {
            base.ValidateCurrentSortingKeyBeforeVisit(sortingKey);
            ValidateCurrentSortingKeyBeforeSingleEntityQueryVisit(sortingKey);
        }

        protected void BuildCurrentPropertyNext<TProperty>()
        {
            if (Parameter == null) throw new ArgumentNullException("Parameter == null");
            if (BuildingPredicateKind != PredicateKind.NextBySingleSortingKey) throw new ApplicationException("BuildingPredicateKind != PredicateKind.NextBySingleSortingKey");
            if (ResultingPredicateCanBeNull) throw new ApplicationException("ResultingPredicateCanBeNull");
            if (CurrentSortingKeyPredecessor != null) throw new ApplicationException("CurrentSortingKeyPredecessor != null");
            if (CurrentSortingKeyPredecessorEqualExpression != null) throw new ApplicationException("CurrentSortingKeyPredecessorEqualExpression != null");

            bool hasPrefixPropertyNext = CurrentSortingKeyPropertyPrefixNextExpression != null;
            bool hasPrefixPropertyEqual = CurrentSortingKeyPropertyPrefixEqualExpression != null;
            bool finalPredicate = CurrentSortingKeyProperty == CurrentSortingKey;
            if (hasPrefixPropertyNext != hasPrefixPropertyEqual) throw new ApplicationException("hasPrefixPropertyNext != hasPrefixPropertyEqual");
            if (hasPrefixPropertyNext && CurrentSortingKeyPropertyPrefix == null)
                throw new ApplicationException("hasPrefixPropertyNext && CurrentSortingKeyPropertyPrefix == null");

            var property = GetCurrentEFProperty<TProperty>();
            var value = GetCurrentSortingKeyPropertyValue();

            CurrentSortingKeyPropertyPrefixNextExpression = hasPrefixPropertyNext
                ? Expression.OrElse(CurrentSortingKeyPropertyPrefixNextExpression,
                    Expression.AndAlso(CurrentSortingKeyPropertyPrefixEqualExpression, GetNextPredicate(property, value)))
                : GetNextPredicate(property, value);

            CurrentSortingKeyPropertyPrefixEqualExpression = finalPredicate
                ? null
                : hasPrefixPropertyEqual
                    ? Expression.AndAlso(CurrentSortingKeyPropertyPrefixEqualExpression, GetEqualPredicate(property, value))
                    : GetEqualPredicate(property, value);
        }

        protected void BuildCurrentPropertyEqual<TProperty>()
        {
            if (Parameter == null) throw new ArgumentNullException("Parameter == null");
            if (BuildingPredicateKind != PredicateKind.EqualByAnySortingKey) throw new ApplicationException("BuildingPredicateKind != PredicateKind.EqualByAnySortingKey");
            if (CurrentSortingKeyPropertyPrefixNextExpression != null) throw new ApplicationException("CurrentSortingKeyPropertyPrefixNextExpression != null");

            bool hasPredecessorEqual = CurrentSortingKeyPredecessorEqualExpression != null;
            bool hasPrefixPropertyEqual = CurrentSortingKeyPropertyPrefixEqualExpression != null;
            bool finalPredicate = CurrentSortingKeyProperty == CurrentSortingKey;
            if (hasPredecessorEqual && CurrentSortingKeyPredecessor == null) throw new ApplicationException("hasPredecessorEqual && CurrentSortingKeyPredecessor == null");
            if (hasPrefixPropertyEqual && CurrentSortingKeyPropertyPrefix == null)
                throw new ApplicationException("hasPrefixPropertyEqual && CurrentSortingKeyPropertyPrefix == null");

            var property = GetCurrentEFProperty<TProperty>();
            var value = GetCurrentSortingKeyPropertyValue();
            if (!ResultingPredicateCanBeNull && property == null) throw new ApplicationException("!ResultingPredicateCanBeNull && property == null");

            if (!ResultingPredicateCanBeNull || property != null)
            {
                CurrentSortingKeyPropertyPrefixEqualExpression = hasPrefixPropertyEqual
                    ? Expression.AndAlso(CurrentSortingKeyPropertyPrefixEqualExpression, GetEqualPredicate(property, value))
                    : GetEqualPredicate(property, value);
            }

            if (finalPredicate)
            {
                CurrentSortingKeyPredecessorEqualExpression
                    = hasPredecessorEqual && CurrentSortingKeyPropertyPrefixEqualExpression != null
                        ? Expression.OrElse(CurrentSortingKeyPredecessorEqualExpression,
                            CurrentSortingKeyPropertyPrefixEqualExpression)
                        : CurrentSortingKeyPredecessorEqualExpression
                            ?? CurrentSortingKeyPropertyPrefixEqualExpression;
                CurrentSortingKeyPropertyPrefixEqualExpression = null;
            }
        }

        protected virtual Expression GetCurrentEFProperty<TProperty>()
        {
            if (Parameter == null) throw new ApplicationException("Parameter == null");
            string propertyName = CurrentSortingKeyProperty?.PropertyName ?? throw new ApplicationException("CurrentSortingKeyProperty?.PropertyName == null");
            return GetCurrentEFPropertySingleEntity<TProperty>(propertyName);
        }

        protected virtual Expression GetCurrentSortingKeyPropertyValue()
        {
            if (Parameter == null) throw new ApplicationException("Parameter == null");
            if (CurrentSortingKeyProperty?.PropertyName == null) throw new ApplicationException("CurrentSortingKeyProperty?.PropertyName == null");
            var closure = Expression.Constant(CurrentSortingKey);
            return Expression.Property(closure, SortingKey.ValuePropertyName);
        }

        protected virtual BinaryExpression GetNextPredicate(Expression property, Expression value)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (Parameter == null) throw new ApplicationException("Parameter == null");
            if (CurrentSortingKeyProperty?.PropertyName == null) throw new ApplicationException("CurrentSortingKeyProperty?.PropertyName == null");
            bool descending = CurrentSortingKeyProperty?.Descending ?? throw new ApplicationException("CurrentSortingKeyProperty == null");
            return descending ? Expression.LessThan(property, value) : Expression.GreaterThan(property, value);
        }

        protected virtual BinaryExpression GetEqualPredicate(Expression property, Expression value)
        {
            if (property == null) throw new ArgumentNullException(nameof(property));
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (Parameter == null) throw new ApplicationException("Parameter == null");
            if (CurrentSortingKeyProperty?.PropertyName == null) throw new ApplicationException("CurrentSortingKeyProperty?.PropertyName == null");
            return Expression.Equal(property, value);
        }

        internal override void ResetVisitState()
        {
            base.ResetVisitState();
            BuildingPredicateKind = PredicateKind.Unspecified;
            CurrentSortingKeyPredecessorEqualExpression = null;
            CurrentSortingKeyPropertyPrefixNextExpression = null;
            CurrentSortingKeyPropertyPrefixEqualExpression = null;
            ResultingPredicateCanBeNull = false;
        }

        internal enum PredicateKind
        {
            Unspecified,
            NextBySingleSortingKey,
            EqualByAnySortingKey
        }
    }
}