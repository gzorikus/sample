using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking
{
    public abstract partial class EntityEntryPropertiesCopyingVisitor
        : SortingKeyTopology.IPrefixFirstPropertiesVisitor
    {
        private bool _skipDefaultValues;

        public ICollationAwareModelProvider ModelProvider { get; set; }
        public IEntityType EntityType { get; set; }

        public void VisitProperty<TValue>(SortingKeyTopology.ILastProperty topology)
            where TValue : IEquatable<TValue>, IComparable<TValue>
        {
            if (topology == null) throw new ArgumentNullException(nameof(topology));
            if (CheckToSkip<TValue>(topology)) return;
            CopyValue<TValue>(topology.PropertyOwner, topology.PropertyName);
        }

        public void VisitProperty<TValue, TModelValue>(SortingKeyTopology.ILastProperty topology)
            where TValue : IEquatable<TValue>, IComparable<TValue>
        {
            if (topology == null) throw new ArgumentNullException(nameof(topology));
            if (CheckToSkip<TValue, TModelValue>(topology)) return;
            CopyValue<TValue, TModelValue>(topology.PropertyOwner, topology.PropertyName);
        }

        protected void UseForCopyAll()
        {
            ModelProvider = null;
            EntityType = null;
            _skipDefaultValues = false;
        }

        protected void UseForCopyNonDefault()
        {
            ModelProvider = null;
            EntityType = null;
            _skipDefaultValues = true;
        }

        protected virtual bool CheckToSkip<TValue>(SortingKeyTopology.ILastProperty topology)
            where TValue : IEquatable<TValue>, IComparable<TValue>
            => false;

        protected virtual bool CheckToSkip<TValue, TModelValue>(SortingKeyTopology.ILastProperty topology)
            where TValue : IEquatable<TValue>, IComparable<TValue>
            => false;

        protected virtual bool CheckToSkip<TValue>(
            SortingKeyTopology.PropertiesOwner propertyOwner,
            string propertyName,
            PropertyEntry sourcePropertyEntry,
            PropertyEntry targetPropertyEntry,
            TValue sourceCurrentValue,
            TValue targetCurrentValue)
            where TValue : IEquatable<TValue>, IComparable<TValue>
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (sourcePropertyEntry == null) throw new ArgumentNullException(nameof(sourcePropertyEntry));
            if (targetPropertyEntry == null) throw new ArgumentNullException(nameof(targetPropertyEntry));

            if (!propertyOwner.Match(sourcePropertyEntry.Metadata.DeclaringType))
                throw new ApplicationException("!propertyOwner.Match(sourcePropertyEntry.Metadata.DeclaringType)");
            if (!propertyOwner.Match(targetPropertyEntry.Metadata.DeclaringType))
                throw new ApplicationException("!propertyOwner.Match(targetPropertyEntry.Metadata.DeclaringType)");
            if (propertyName != sourcePropertyEntry.Metadata.Name)
                throw new ApplicationException("propertyName != sourcePropertyEntry.Metadata.Name");
            if (propertyName != targetPropertyEntry.Metadata.Name)
                throw new ApplicationException("propertyName != targetPropertyEntry.Metadata.Name");

            var modelProvider = ModelProvider ?? throw new ApplicationException("ModelProvider == null");
            bool skipDefaultValue = _skipDefaultValues
                && modelProvider.GetModelEqualityComparerRequired<TValue>(propertyOwner, propertyName)
                    .Equals(sourceCurrentValue, default);
            bool skipAlreadyModifiedPropertyWithEqualValue = targetPropertyEntry.IsModified
                && modelProvider.GetModelEqualityComparerRequired<TValue>(propertyOwner, propertyName)
                    .Equals(sourceCurrentValue, targetCurrentValue);
            return skipDefaultValue || skipAlreadyModifiedPropertyWithEqualValue;
        }

        protected abstract void CopyValue<TValue>(
            SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName)
            where TValue : IEquatable<TValue>, IComparable<TValue>;

        protected abstract void CopyValue<TValue, TModelValue>(
            SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName)
            where TValue : IEquatable<TValue>, IComparable<TValue>;

        protected virtual bool CheckToSkip<TValue, TModelValue>(
            SortingKeyTopology.PropertiesOwner propertyOwner,
            string propertyName,
            PropertyEntry sourcePropertyEntry,
            PropertyEntry targetPropertyEntry,
            TModelValue sourceCurrentValue,
            TModelValue targetCurrentValue)
            where TValue : IEquatable<TValue>, IComparable<TValue>
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (sourcePropertyEntry == null) throw new ArgumentNullException(nameof(sourcePropertyEntry));
            if (targetPropertyEntry == null) throw new ArgumentNullException(nameof(targetPropertyEntry));

            if (!propertyOwner.Match(sourcePropertyEntry.Metadata.DeclaringType))
                throw new ApplicationException("!propertyOwner.Match(sourcePropertyEntry.Metadata.DeclaringType)");
            if (!propertyOwner.Match(targetPropertyEntry.Metadata.DeclaringType))
                throw new ApplicationException("!propertyOwner.Match(targetPropertyEntry.Metadata.DeclaringType)");
            if (propertyName != sourcePropertyEntry.Metadata.Name)
                throw new ApplicationException("propertyName != sourcePropertyEntry.Metadata.Name");
            if (propertyName != targetPropertyEntry.Metadata.Name)
                throw new ApplicationException("propertyName != targetPropertyEntry.Metadata.Name");

            var modelProvider = ModelProvider ?? throw new ApplicationException("ModelProvider == null");
            bool skipDefaultValue = _skipDefaultValues
                && modelProvider.GetModelEqualityComparerRequired<TModelValue>(propertyOwner, propertyName)
                    .Equals(sourceCurrentValue, default);
            bool skipAlreadyModifiedPropertyWithEqualValue = targetPropertyEntry.IsModified
                && modelProvider.GetModelEqualityComparerRequired<TModelValue>(propertyOwner, propertyName)
                    .Equals(sourceCurrentValue, targetCurrentValue);
            return skipDefaultValue || skipAlreadyModifiedPropertyWithEqualValue;
        }
    }
}