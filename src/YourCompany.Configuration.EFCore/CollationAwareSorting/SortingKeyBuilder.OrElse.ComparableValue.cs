using System;
using System.Collections.Generic;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public readonly partial struct SortingKeyBuilder<TValue>
    {
        public readonly partial struct OrElse
        {
            public readonly struct ComparableValue<TProviderValue>
                where TProviderValue : IEquatable<TProviderValue>, IComparable<TProviderValue>
            {
                internal OrElse OrElse { get; init; }
                internal SortingKeyBuilder<TValue>.ComparableValue<TProviderValue> KeyBuilder { get; init; }

                public SortingKeyBuilder<TAddValue>.OrElse AndWithSameOwnerProperty<TAddValue>(
                    string name, TAddValue value, bool descending = false)
                {
                    var owner = KeyBuilder.KeyBuilder._owner;
                    return AndWithProperty(owner, name, value, descending);
                }

                public SortingKeyBuilder<TAddValue>.OrElse AndWithProperty<TAddValue>(
                    Type entityType, string name, TAddValue value, bool descending = false)
                {
                    var owner = new SortingKeyTopology.PropertiesOwner { EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType)) };
                    return AndWithProperty(owner, name, value, descending);
                }

                public SortingKeyBuilder<TAddValue>.OrElse AndWithProperty<TAddValue>(
                    string entityName, string name, TAddValue value, bool descending = false, Type matchSharedEntityType = null)
                {
                    var owner = new SortingKeyTopology.PropertiesOwner
                    {
                        EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName)),
                        EntityType = matchSharedEntityType
                    };
                    return AndWithProperty(owner, name, value, descending);
                }

                public SortingKeyQueries.ISingleEntityEquality ForSingleEntityEquality(string replacingQueriedSetName = null)
                {
                    if (OrElse._keys == null) throw new ApplicationException("OrElse._keys == null");
                    if (OrElse._keys.ReplaceQueriedSetName != null) throw new ApplicationException("OrElse._keys.ReplaceQueriedSetName != null");
                    if (OrElse._keys.EntitiesValueTuplePropertyOwnerIndecies != null) throw new ApplicationException("OrElse._keys.EntitiesValueTuplePropertyOwnerIndecies != null");
                    OrElse._keys.AddAndTryRetainSingleTopology(KeyBuilder.CreateIntermediatePropertySortingKey());
                    OrElse._keys.ProtectFromChanges();
                    OrElse._keys.ReplaceQueriedSetName = replacingQueriedSetName;
                    OrElse._keys.EnsureCompatibleWithSingleEntityQueries();
                    return OrElse._keys;
                }

                public SortingKeyQueries.IMultiTopologyMultiEntityQuery ForMultiEntityEquality(
                    IReadOnlyList<SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex>
                        entitiesValueTuplePropertyOwnerIndecies)
                {
                    if (entitiesValueTuplePropertyOwnerIndecies == null)
                        throw new ApplicationException("entitiesValueTuplePropertyOwnerIndecies == null");
                    if (entitiesValueTuplePropertyOwnerIndecies.Count == 0)
                        throw new ApplicationException("entitiesValueTuplePropertyOwnerIndecies.Count == 0");

                    if (OrElse._keys == null) throw new ApplicationException("OrElse._keys == null");
                    if (OrElse._keys.ReplaceQueriedSetName != null) throw new ApplicationException("OrElse._keys.ReplaceQueriedSetName != null");
                    if (OrElse._keys.EntitiesValueTuplePropertyOwnerIndecies != null) throw new ApplicationException("OrElse._keys.EntitiesValueTuplePropertyOwnerIndecies != null");
                    OrElse._keys.AddAndTryRetainSingleTopology(KeyBuilder.CreateIntermediatePropertySortingKey());
                    OrElse._keys.ProtectFromChanges();
                    OrElse._keys.EntitiesValueTuplePropertyOwnerIndecies = entitiesValueTuplePropertyOwnerIndecies;
                    return OrElse._keys;
                }

                public SortingKeyBuilder<TAddValue>.OrElse AddAnotherKeyWithSameOwnerProperty<TAddValue>(
                    string name, TAddValue value, bool descending = false)
                {
                    var owner = KeyBuilder.KeyBuilder._owner;
                    return AddAnotherKeyWithProperty(owner, name, value, descending);
                }

                public SortingKeyBuilder<TAddValue>.OrElse AddAnotherKeyWithProperty<TAddValue>(
                    Type entityType, string name, TAddValue value, bool descending = false)
                {
                    var owner = new SortingKeyTopology.PropertiesOwner { EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType)) };
                    return AddAnotherKeyWithProperty(owner, name, value, descending);
                }

                public SortingKeyBuilder<TAddValue>.OrElse AddAnotherKeyWithProperty<TAddValue>(
                    string entityName, string name, TAddValue value, bool descending = false, Type matchSharedEntityType = null)
                {
                    var owner = new SortingKeyTopology.PropertiesOwner
                    {
                        EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName)),
                        EntityType = matchSharedEntityType
                    };
                    return AddAnotherKeyWithProperty(owner, name, value, descending);
                }

                private SortingKeyBuilder<TAddValue>.OrElse AndWithProperty<TAddValue>(
                    SortingKeyTopology.PropertiesOwner owner, string name, TAddValue value, bool descending)
                {
                    if (owner == default) throw new ArgumentNullException(nameof(owner));
                    if (OrElse._keys == null) throw new ApplicationException("OrElse._keys == null");
                    if (OrElse._keys.ReplaceQueriedSetName != null) throw new ApplicationException("OrElse._keys.ReplaceQueriedSetName != null");
                    if (OrElse._keys.EntitiesValueTuplePropertyOwnerIndecies != null) throw new ApplicationException("OrElse._keys.EntitiesValueTuplePropertyOwnerIndecies != null");
                    var newBuilder = new SortingKeyBuilder<TAddValue>(
                        KeyBuilder.KeyBuilder._modelProvider, owner, name, value, descending)
                    {
                        PrefixKey = KeyBuilder.CreateIntermediatePropertySortingKey()
                    };
                    return new(OrElse._keys) { KeyBuilder = newBuilder };
                }

                private SortingKeyBuilder<TAddValue>.OrElse AddAnotherKeyWithProperty<TAddValue>(
                    SortingKeyTopology.PropertiesOwner owner, string name, TAddValue value, bool descending)
                {
                    if (owner == default) throw new ArgumentNullException(nameof(owner));
                    if (OrElse._keys == null) throw new ApplicationException("OrElse._keys == null");
                    if (OrElse._keys.ReplaceQueriedSetName != null) throw new ApplicationException("OrElse._keys.ReplaceQueriedSetName != null");
                    if (OrElse._keys.EntitiesValueTuplePropertyOwnerIndecies != null) throw new ApplicationException("OrElse._keys.EntitiesValueTuplePropertyOwnerIndecies != null");
                    var newBuilder = new SortingKeyBuilder<TAddValue>(
                        KeyBuilder.KeyBuilder._modelProvider, owner, name, value, descending);
                    OrElse._keys.AddAndTryRetainSingleTopology(KeyBuilder.CreateIntermediatePropertySortingKey());
                    return new(OrElse._keys) { KeyBuilder = newBuilder };
                }
            }
        }
    }
}