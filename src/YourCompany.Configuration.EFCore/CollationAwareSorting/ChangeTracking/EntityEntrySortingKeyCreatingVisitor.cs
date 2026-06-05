using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking
{
    public abstract partial class EntityEntrySortingKeyCreatingVisitor
        : SortingKeyTopology.IPrefixFirstPropertiesVisitor
    {
        public ICollationAwareModelProvider ModelProvider { get; set; }
        public SortingKey Prefix { get; private set; }
        public string LastSortingKeyReplacingQueriedSetName { get; private set; }

        public void VisitProperty<TValue>(SortingKeyTopology.ILastProperty topology)
            where TValue : IEquatable<TValue>, IComparable<TValue>
        {
            if (topology == null) throw new ArgumentNullException(nameof(topology));
            if (!CheckToSkip<TValue>(topology)) CreateSortingKey<TValue>(topology);
        }

        public void VisitProperty<TValue, TModelValue>(SortingKeyTopology.ILastProperty topology)
            where TValue : IEquatable<TValue>, IComparable<TValue>
        {
            if (topology == null) throw new ArgumentNullException(nameof(topology));
            if (!CheckToSkip<TValue, TModelValue>(topology)) CreateSortingKey<TValue, TModelValue>(topology);
        }

        protected void UseForNewSortingKey()
        {
            Prefix = null;
            ModelProvider = null;
            LastSortingKeyReplacingQueriedSetName = null;
        }

        protected void UseForPropertiesAdding(SortingKey prefix)
        {
            if (prefix == null) throw new ArgumentNullException(nameof(prefix));
            Prefix = prefix.ToIntermediate();
            ModelProvider = null;
            LastSortingKeyReplacingQueriedSetName = null;
        }

        protected virtual bool CheckToSkip<TValue>(SortingKeyTopology.ILastProperty topology)
            where TValue : IEquatable<TValue>, IComparable<TValue>
            => false;

        protected virtual bool CheckToSkip<TValue, TModelValue>(SortingKeyTopology.ILastProperty topology)
            where TValue : IEquatable<TValue>, IComparable<TValue>
            => false;

        protected virtual bool CheckToSkip<TValue>(PropertyEntry propertyEntry, TValue currentValue)
            where TValue : IEquatable<TValue>, IComparable<TValue>
            => false;

        protected virtual bool CheckToSkip<TValue, TModelValue>(PropertyEntry propertyEntry, TModelValue currentValue)
            where TValue : IEquatable<TValue>, IComparable<TValue>
            => false;

        protected abstract void CreateSortingKey<TValue>(SortingKeyTopology.ILastProperty topology)
            where TValue : IEquatable<TValue>, IComparable<TValue>;

        protected abstract void CreateSortingKey<TValue, TModelValue>(SortingKeyTopology.ILastProperty topology)
            where TValue : IEquatable<TValue>, IComparable<TValue>;

        protected SortingKey CreateSortingKey<TValue>(
            SortingKeyTopology.PropertiesOwner propertyOwner,
            string propertyName,
            bool descending,
            EntityEntry propertiesOwner)
            where TValue : IEquatable<TValue>, IComparable<TValue>
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (propertiesOwner == null) throw new ArgumentNullException(nameof(propertiesOwner));
            var value = GetValue<TValue>(propertiesOwner, propertyName);
            if (CheckToSkip(propertiesOwner.Property(propertyName), value)) return Prefix;
            var key = new SortingKey.ValueHolding<TValue>(ModelProvider, Prefix);
            key.SetValue(propertyOwner, propertyName, value, descending);
            RemoveSuggestedQueryOptions(key);
            return key;
        }

        protected SortingKey CreateSortingKey<TValue, TModelValue>(
            SortingKeyTopology.PropertiesOwner propertyOwner,
            string propertyName,
            bool descending,
            EntityEntry propertiesOwner)
            where TValue : IEquatable<TValue>, IComparable<TValue>
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (propertiesOwner == null) throw new ArgumentNullException(nameof(propertiesOwner));
            var value = GetValue<TModelValue>(propertiesOwner, propertyName);
            if (CheckToSkip<TValue, TModelValue>(propertiesOwner.Property(propertyName), value)) return Prefix;
            var key = new SortingKey.ValueHolding<TValue>.ConvertedFrom<TModelValue>(ModelProvider, Prefix);
            key.SetValue(propertyOwner, propertyName, value, descending);
            RemoveSuggestedQueryOptions(key);
            return key;
        }

        protected abstract TProperty GetValue<TProperty>(EntityEntry propertiesOwner, string propertyName);

        private void RemoveSuggestedQueryOptions(SortingKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            key.EnsureNoUnexpectedQueryOptionsAfterCreation();
            LastSortingKeyReplacingQueriedSetName = key.ReplaceQueriedSetName;
            key.ReplaceQueriedSetName = null;
            if (key.EntitiesValueTuplePropertyOwnerIndecies != null)
                throw new ApplicationException("key.EntitiesValueTuplePropertyOwnerIndecies != null");
        }
    }
}