using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking
{
    public abstract partial class EntityEntryPropertiesCopyingVisitor
    {
        public class Generic<TEntity> : EntityEntryPropertiesCopyingVisitor where TEntity : class
        {
            public EntityEntry<TEntity> SourcePropertiesOwner { get; private set; }
            public EntityEntry<TEntity> TargetPropertiesOwner { get; private set; }

            public Generic<TEntity> CopyAll(
                SortingKeyTopology.ILastProperty topology,
                EntityEntry<TEntity> sourcePropertiesOwner,
                EntityEntry<TEntity> targetPropertiesOwner)
            {
                if (topology == null) throw new ArgumentNullException(nameof(topology));
                topology.VisitPrefixFirst(UseForCopyAll(sourcePropertiesOwner, targetPropertiesOwner));
                return this;
            }

            public Generic<TEntity> UseForCopyAll(
                EntityEntry<TEntity> sourcePropertiesOwner, EntityEntry<TEntity> targetPropertiesOwner)
            {
                SourcePropertiesOwner = sourcePropertiesOwner ?? throw new ArgumentNullException(nameof(sourcePropertiesOwner));
                TargetPropertiesOwner = targetPropertiesOwner ?? throw new ArgumentNullException(nameof(targetPropertiesOwner));
                UseForCopyAll();
                return this;
            }

            public Generic<TEntity> CopyNonDefault(
                SortingKeyTopology.ILastProperty topology,
                EntityEntry<TEntity> sourcePropertiesOwner,
                EntityEntry<TEntity> targetPropertiesOwner)
            {
                if (topology == null) throw new ArgumentNullException(nameof(topology));
                topology.VisitPrefixFirst(UseForCopyNonDefault(sourcePropertiesOwner, targetPropertiesOwner));
                return this;
            }

            public Generic<TEntity> UseForCopyNonDefault(
                EntityEntry<TEntity> sourcePropertiesOwner, EntityEntry<TEntity> targetPropertiesOwner)
            {
                SourcePropertiesOwner = sourcePropertiesOwner ?? throw new ArgumentNullException(nameof(sourcePropertiesOwner));
                TargetPropertiesOwner = targetPropertiesOwner ?? throw new ArgumentNullException(nameof(targetPropertiesOwner));
                UseForCopyNonDefault();
                return this;
            }

            protected override void CopyValue<TValue>(
                SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName)
                => CopyValue<TValue>(propertyOwner, propertyName, SourcePropertiesOwner, TargetPropertiesOwner);

            protected override void CopyValue<TValue, TModelValue>(
                SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName)
                => CopyValue<TValue, TModelValue>(propertyOwner, propertyName, SourcePropertiesOwner, TargetPropertiesOwner);

            private void CopyValue<TValue>(
                SortingKeyTopology.PropertiesOwner propertyOwner,
                string propertyName,
                EntityEntry<TEntity> sourcePropertiesOwner,
                EntityEntry<TEntity> targetPropertiesOwner)
                where TValue : IEquatable<TValue>, IComparable<TValue>
            {
                if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
                if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
                if (sourcePropertiesOwner == null) throw new ArgumentNullException(nameof(sourcePropertiesOwner));
                if (targetPropertiesOwner == null) throw new ArgumentNullException(nameof(targetPropertiesOwner));
                var sourceProperty = sourcePropertiesOwner.Property<TValue>(propertyName);
                var targetProperty = targetPropertiesOwner.Property<TValue>(propertyName);
                var sourceValue = sourceProperty.CurrentValue;
                var targetValue = targetProperty.CurrentValue;
                if (!CheckToSkip(propertyOwner, propertyName, sourceProperty, targetProperty, sourceValue, targetValue))
                {
                    if (targetProperty.IsModified) throw new ApplicationException("targetProperty.IsModified");
                    targetProperty.CurrentValue = sourceValue;
                }
            }

            private void CopyValue<TValue, TModelValue>(
                SortingKeyTopology.PropertiesOwner propertyOwner,
                string propertyName,
                EntityEntry<TEntity> sourcePropertiesOwner,
                EntityEntry<TEntity> targetPropertiesOwner)
                where TValue : IEquatable<TValue>, IComparable<TValue>
            {
                if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
                if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
                if (sourcePropertiesOwner == null) throw new ArgumentNullException(nameof(sourcePropertiesOwner));
                if (targetPropertiesOwner == null) throw new ArgumentNullException(nameof(targetPropertiesOwner));
                var sourceProperty = sourcePropertiesOwner.Property<TModelValue>(propertyName);
                var targetProperty = targetPropertiesOwner.Property<TModelValue>(propertyName);
                var sourceValue = sourceProperty.CurrentValue;
                var targetValue = targetProperty.CurrentValue;
                if (!CheckToSkip<TValue, TModelValue>(
                    propertyOwner, propertyName, sourceProperty, targetProperty, sourceValue, targetValue))
                {
                    if (targetProperty.IsModified) throw new ApplicationException("targetProperty.IsModified");
                    targetProperty.CurrentValue = sourceValue;
                }
            }
        }
    }
}