using System;
using System.Collections.Generic;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public abstract partial class SortingKey
    {
        public SortingKeyBuilder<TValue> WithSameOwnerProperty<TValue>(
            string name, TValue value, bool descending = false)
            => new(ModelProvider, PropertyOwner, name, value, descending) { PrefixKey = ToIntermediate() };

        public SortingKeyBuilder<TValue> WithProperty<TValue>(
            Type entityType, string name, TValue value, bool descending = false)
        {
            var owner = new SortingKeyTopology.PropertiesOwner { EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType)) };
            return new(ModelProvider, owner, name, value, descending) { PrefixKey = ToIntermediate() };
        }

        public SortingKeyBuilder<TValue> WithProperty<TValue>(
            string entityName, string name, TValue value, bool descending = false, Type matchSharedEntityType = null)
        {
            var owner = new SortingKeyTopology.PropertiesOwner
            {
                EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName)),
                EntityType = matchSharedEntityType
            };
            return new(ModelProvider, owner, name, value, descending) { PrefixKey = ToIntermediate() };
        }

        public SortingKeyBuilder<TValue>.OrElse OrAnotherKeyForEqualityOnlyWithSameOwnerProperty<TValue>(
            string name, TValue value, bool descending = false)
            => new(ToIntermediate())
            {
                KeyBuilder = new SortingKeyBuilder<TValue>(ModelProvider, PropertyOwner, name, value, descending)
            };

        public SortingKeyBuilder<TValue>.OrElse OrAnotherKeyForEqualityOnlyWithProperty<TValue>(
            Type entityType, string name, TValue value, bool descending = false)
        {
            var owner = new SortingKeyTopology.PropertiesOwner { EntityType = entityType ?? throw new ArgumentNullException(nameof(entityType)) };
            return new(ToIntermediate())
            {
                KeyBuilder = new SortingKeyBuilder<TValue>(ModelProvider, owner, name, value, descending)
            };
        }

        public SortingKeyBuilder<TValue>.OrElse OrAnotherKeyForEqualityOnlyWithProperty<TValue>(
            string entityName, string name, TValue value, bool descending = false, Type matchSharedEntityType = null)
        {
            var owner = new SortingKeyTopology.PropertiesOwner
            {
                EntityName = entityName ?? throw new ArgumentNullException(nameof(entityName)),
                EntityType = matchSharedEntityType
            };
            return new(ToIntermediate())
            {
                KeyBuilder = new SortingKeyBuilder<TValue>(ModelProvider, owner, name, value, descending)
            };
        }

        public SortingKey ToIntermediate() => WithDescending(Descending, overrideSingleEntityReplacingQueriedSetName: null);

        public SortingKey WithDescending(
            bool descending,
            SortingKey descendingForPrefix = null,
            string overrideSingleEntityReplacingQueriedSetName = "never matching any set name → no need to override",
            IReadOnlyList<SortingKeyQueries.MultiEntityQuerySortingKeyPropertyOwnerIndex>
                overrideMultiEntityValueTuplePropertyOwnerIndecies = null)
        {
            if (PropertyName == null) throw new ApplicationException("PropertyName == null");
            bool overrideSingleEntity = overrideSingleEntityReplacingQueriedSetName != "never matching any set name → no need to override";
            bool overrideMultiEntity = overrideMultiEntityValueTuplePropertyOwnerIndecies != null;
            if (overrideSingleEntity && overrideMultiEntity) throw new ApplicationException("overrideSingleEntity && overrideMultiEntity");
            if (overrideMultiEntityValueTuplePropertyOwnerIndecies?.Count == 0) throw new ApplicationException("overrideMultiEntityValueTuplePropertyOwnerIndecies?.Count == 0");

            SortingKey clonedPrefix = null;

            if (descendingForPrefix != null && descendingForPrefix != this)
            {
                if (PrefixKey == null) throw new ApplicationException("forPrefix != null && PrefixKey == null");
                var possiblyClonedPrefix = PrefixKey.WithDescending(descending, descendingForPrefix);
                if (!ReferenceEquals(possiblyClonedPrefix, PrefixKey))
                {
                    clonedPrefix = possiblyClonedPrefix;
                    descending = Descending;
                }
            }

            if (clonedPrefix == null && descending == Descending && !overrideSingleEntity && !overrideMultiEntity)
                return this;

            var clone = Clone(descending, clonedPrefix);
            clone.EnsureNoUnexpectedQueryOptionsAfterCreation();
            clone.ReplaceQueriedSetName = ReplaceQueriedSetName;
            clone.EntitiesValueTuplePropertyOwnerIndecies = EntitiesValueTuplePropertyOwnerIndecies;

            if (overrideSingleEntity)
            {
                clone.ReplaceQueriedSetName = overrideSingleEntityReplacingQueriedSetName;
                clone.EntitiesValueTuplePropertyOwnerIndecies = null;
            }

            if (overrideMultiEntity)
            {
                clone.ReplaceQueriedSetName = null;
                clone.EntitiesValueTuplePropertyOwnerIndecies = overrideMultiEntityValueTuplePropertyOwnerIndecies;
            }

            return clone;
        }

        protected abstract SortingKey Clone(bool descending, SortingKey clonedPrefix);
    }
}