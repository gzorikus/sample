using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking
{
    internal abstract partial class EntityEntryPropertiesSettingVisitor
    {
        internal class Generic<TEntity> : EntityEntryPropertiesSettingVisitor where TEntity : class
        {
            internal EntityEntry<TEntity> PropertiesOwner { get; private set; }

            internal Generic<TEntity> SetSingleEntityQueryValuesTo(
                SortingKey sortingKey, EntityEntry<TEntity> propertiesOwner)
            {
                if (sortingKey == null) throw new ArgumentNullException(nameof(sortingKey));
                ResetVisitState();
                SetNextSortingKey(sortingKey);
                sortingKey.VisitPrefixFirst(UseForSingleEntityQuery(propertiesOwner));
                return this;
            }

            internal Generic<TEntity> UseForSingleEntityQuery(EntityEntry<TEntity> propertiesOwner)
            {
                PropertiesOwner = propertiesOwner ?? throw new ArgumentNullException(nameof(propertiesOwner));
                UseForAllPropertiesSetting();
                return this;
            }

            protected override void VisitCurrentProperty<TProperty>(SortingKey property, TProperty value)
            {
                base.VisitCurrentProperty(property, value);
                if (!CheckToSkip(property, value))
                    SetValue(property.PropertyOwner, property.PropertyName, value, PropertiesOwner);
            }

            protected override TProperty GetValue<TProperty>(EntityEntry propertiesOwner, string propertyName)
            {
                var entry = (EntityEntry<TEntity>)propertiesOwner ?? throw new ArgumentNullException(nameof(propertiesOwner));
                return entry.Property<TProperty>(propertyName).CurrentValue;
            }

            protected override void SetValue<TProperty>(
                SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName, TProperty value)
                => SetValue(propertyOwner, propertyName, value, PropertiesOwner);

            private void SetValue<TProperty>(
                SortingKeyTopology.PropertiesOwner propertyOwner,
                string propertyName,
                TProperty value,
                EntityEntry<TEntity> propertiesOwner)
            {
                if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
                if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
                if (propertiesOwner == null) throw new ArgumentNullException(nameof(propertiesOwner));
                if (!CheckToSkip(propertyOwner, propertyName, value, propertiesOwner))
                    propertiesOwner.Property<TProperty>(propertyName).CurrentValue = value;
            }
        }
    }
}