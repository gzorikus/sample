using System;
using System.Collections.Generic;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public readonly partial struct SortingKeyBuilder<TValue>
    {
        public readonly struct ComparableValue<TProviderValue>
            where TProviderValue : IEquatable<TProviderValue>, IComparable<TProviderValue>
        {
            internal SortingKeyBuilder<TValue> KeyBuilder { get; init; }

            public SortingKeyBuilder<TValue> AndWithSameOwnerProperty(
                string name, TValue value, bool descending = false)
                => new(KeyBuilder._modelProvider, KeyBuilder._owner, name, value, descending)
                {
                    PrefixKey = CreateIntermediatePropertySortingKey()
                };

            public SortingKeyBuilder<TAddValue> AndWithProperty<TAddValue>(
                Type entityType, string name, TAddValue value, bool descending = false)
                where TAddValue : IEquatable<TAddValue>, IComparable<TAddValue>
            {
                var owner = new SortingKeyTopology.PropertiesOwner { EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType)) };
                return new(KeyBuilder._modelProvider, owner, name, value, descending)
                {
                    PrefixKey = CreateIntermediatePropertySortingKey()
                };
            }

            public SortingKeyBuilder<TAddValue> AndWithProperty<TAddValue>(
                string entityName, string name, TAddValue value, bool descending = false, Type matchSharedEntityType = null)
                where TAddValue : IEquatable<TAddValue>, IComparable<TAddValue>
            {
                var owner = new SortingKeyTopology.PropertiesOwner
                {
                    EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName)),
                    EntityType = matchSharedEntityType
                };
                return new(KeyBuilder._modelProvider, owner, name, value, descending)
                {
                    PrefixKey = CreateIntermediatePropertySortingKey()
                };
            }

            public SortingKey.ValueHolding<TProviderValue> ForSingleEntityQueries(string replacingQueriedSetName = null)
            {
                var sortingKey = CreateSortingKey();
                sortingKey.ReplaceQueriedSetName = replacingQueriedSetName ?? sortingKey.ReplaceQueriedSetName;
                sortingKey.EntitiesValueTuplePropertyOwnerIndecies = null;
                sortingKey.EnsureCompatibleWithSingleEntityQueries(
                    singleEntityReplacingQueriedSet: sortingKey.ReplaceQueriedSetName != null);
                return sortingKey;
            }

            public SortingKey.ValueHolding<TProviderValue> ForMultiEntityQueries(
                IReadOnlyList<SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex>
                    entitiesValueTuplePropertyOwnerIndecies)
            {
                if (entitiesValueTuplePropertyOwnerIndecies == null)
                    throw new ArgumentNullException(nameof(entitiesValueTuplePropertyOwnerIndecies));
                if (entitiesValueTuplePropertyOwnerIndecies.Count == 0)
                    throw new ApplicationException("entitiesValueTuplePropertyOwnerIndecies.Count == 0");

                var sortingKey = CreateSortingKey();
                sortingKey.ReplaceQueriedSetName = null;
                sortingKey.EntitiesValueTuplePropertyOwnerIndecies = entitiesValueTuplePropertyOwnerIndecies;
                return sortingKey;
            }

            public SortingKeyBuilder<TAddValue>.OrElse OrAddAnotherKeyForEqualityOnlyWithProperty<TAddValue>(
                SortingKeyTopology.PropertiesOwner owner, string name, TAddValue value, bool descending = false)
                => new(CreateIntermediatePropertySortingKey())
                {
                    KeyBuilder = new SortingKeyBuilder<TAddValue>(
                        KeyBuilder._modelProvider, owner, name, value, descending)
                };

            internal SortingKey.ValueHolding<TProviderValue> CreateIntermediatePropertySortingKey()
            {
                var sortingKey = CreateSortingKey();
                sortingKey.ReplaceQueriedSetName = null;
                sortingKey.EntitiesValueTuplePropertyOwnerIndecies = null;
                return sortingKey;
            }

            private SortingKey.ValueHolding<TProviderValue> CreateSortingKey()
            {
                SortingKey.ValueHolding<TProviderValue> sortingKey;

                if (typeof(TProviderValue) == typeof(TValue))
                {
                    if (KeyBuilder._value is not TProviderValue valueOfSameType)
                        throw new ApplicationException("KeyBuilder._value is not TProviderValue valueOfSameType");
                    var sameValueType = new SortingKey.ValueHolding<TProviderValue>(KeyBuilder._modelProvider, KeyBuilder.PrefixKey);
                    sameValueType.SetValue(KeyBuilder._owner, KeyBuilder._name, valueOfSameType, KeyBuilder._descending);
                    sortingKey = sameValueType;
                }
                else
                {
                    var convertedValueType = new SortingKey.ValueHolding<TProviderValue>.ConvertedFrom<TValue>(
                        KeyBuilder._modelProvider, KeyBuilder.PrefixKey);
                    convertedValueType.SetValue(KeyBuilder._owner, KeyBuilder._name, KeyBuilder._value, KeyBuilder._descending);
                    sortingKey = convertedValueType;
                }

                sortingKey.EnsureNoUnexpectedQueryOptionsAfterCreation();
                return sortingKey;
            }
        }
    }
}