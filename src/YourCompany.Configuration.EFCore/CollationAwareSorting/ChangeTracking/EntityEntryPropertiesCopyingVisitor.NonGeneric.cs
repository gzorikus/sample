using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using YourCompany.Configuration.EFCore.ChangeTracking;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking
{
    internal abstract partial class EntityEntryPropertiesCopyingVisitor
    {
        internal class NonGeneric : EntityEntryPropertiesCopyingVisitor
        {
            internal EntityEntry SourcePropertiesOwner { get; private set; }
            internal EntityEntry TargetPropertiesOwner { get; private set; }

            internal NonGeneric CopyAll(
                SortingKeyTopology.ILastProperty topology,
                EntityEntry sourcePropertiesOwner,
                EntityEntry targetPropertiesOwner)
            {
                if (topology == null) throw new ArgumentNullException(nameof(topology));
                topology.VisitPrefixFirst(UseForCopyAll(sourcePropertiesOwner, targetPropertiesOwner));
                return this;
            }

            internal NonGeneric UseForCopyAll(EntityEntry sourcePropertiesOwner, EntityEntry targetPropertiesOwner)
            {
                SourcePropertiesOwner = sourcePropertiesOwner ?? throw new ArgumentNullException(nameof(sourcePropertiesOwner));
                TargetPropertiesOwner = targetPropertiesOwner ?? throw new ArgumentNullException(nameof(targetPropertiesOwner));
                UseForCopyAll();
                return this;
            }

            internal NonGeneric CopyNonDefault(
                SortingKeyTopology.ILastProperty topology,
                EntityEntry sourcePropertiesOwner,
                EntityEntry targetPropertiesOwner)
            {
                if (topology == null) throw new ArgumentNullException(nameof(topology));
                topology.VisitPrefixFirst(UseForCopyNonDefault(sourcePropertiesOwner, targetPropertiesOwner));
                return this;
            }

            internal NonGeneric UseForCopyNonDefault(EntityEntry sourcePropertiesOwner, EntityEntry targetPropertiesOwner)
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
                EntityEntry sourcePropertiesOwner,
                EntityEntry targetPropertiesOwner)
                where TValue : IEquatable<TValue>, IComparable<TValue>
            {
                if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
                if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
                if (sourcePropertiesOwner == null) throw new ArgumentNullException(nameof(sourcePropertiesOwner));
                if (targetPropertiesOwner == null) throw new ArgumentNullException(nameof(targetPropertiesOwner));

                var sourceEntryEntityType = sourcePropertiesOwner.Metadata.ClrType ?? throw new ApplicationException("sourcePropertiesOwner.Metadata.ClrType == null");
                var targetEntryEntityType = targetPropertiesOwner.Metadata.ClrType ?? throw new ApplicationException("targetPropertiesOwner.Metadata.ClrType == null");

                var sourceProperty = sourcePropertiesOwner.Property(propertyName);
                var targetProperty = targetPropertiesOwner.Property(propertyName);
                var getter = EFEntityEntryPropertiesCache<TValue>.GetGetter(sourceEntryEntityType, propertyName);
                var setter = EFEntityEntryPropertiesCache<TValue>.GetSetter(targetEntryEntityType, propertyName);
                var sourceValue = getter(sourcePropertiesOwner);
                var targetValue = getter(targetPropertiesOwner);
                if (!CheckToSkip(propertyOwner, propertyName, sourceProperty, targetProperty, sourceValue, targetValue))
                {
                    if (targetProperty.IsModified) throw new ApplicationException("targetProperty.IsModified");
                    setter(targetPropertiesOwner, sourceValue);
                }
            }

            private void CopyValue<TValue, TModelValue>(
                SortingKeyTopology.PropertiesOwner propertyOwner,
                string propertyName,
                EntityEntry sourcePropertiesOwner,
                EntityEntry targetPropertiesOwner)
                where TValue : IEquatable<TValue>, IComparable<TValue>
            {
                if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
                if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
                if (sourcePropertiesOwner == null) throw new ArgumentNullException(nameof(sourcePropertiesOwner));
                if (targetPropertiesOwner == null) throw new ArgumentNullException(nameof(targetPropertiesOwner));

                var sourceEntryEntityType = sourcePropertiesOwner.Metadata.ClrType ?? throw new ApplicationException("sourcePropertiesOwner.Metadata.ClrType == null");
                var targetEntryEntityType = targetPropertiesOwner.Metadata.ClrType ?? throw new ApplicationException("targetPropertiesOwner.Metadata.ClrType == null");

                var sourceProperty = sourcePropertiesOwner.Property(propertyName);
                var targetProperty = targetPropertiesOwner.Property(propertyName);
                var getter = EFEntityEntryPropertiesCache<TModelValue>.GetGetter(sourceEntryEntityType, propertyName);
                var setter = EFEntityEntryPropertiesCache<TModelValue>.GetSetter(targetEntryEntityType, propertyName);
                var sourceValue = getter(sourcePropertiesOwner);
                var targetValue = getter(targetPropertiesOwner);
                if (!CheckToSkip<TValue, TModelValue>(
                    propertyOwner, propertyName, sourceProperty, targetProperty, sourceValue, targetValue))
                {
                    if (targetProperty.IsModified) throw new ApplicationException("targetProperty.IsModified");
                    setter(targetPropertiesOwner, sourceValue);
                }
            }
        }
    }
}