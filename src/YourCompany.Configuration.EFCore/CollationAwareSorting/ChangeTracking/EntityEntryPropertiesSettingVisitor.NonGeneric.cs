using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using YourCompany.Configuration.EFCore.ChangeTracking;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking
{
    internal abstract partial class EntityEntryPropertiesSettingVisitor
    {
        internal class NonGeneric : EntityEntryPropertiesSettingVisitor
        {
            internal EntityEntry PropertiesOwner { get; private set; }
            public IReadOnlyList<EntityEntry> PropertyOwners { get; private set; }

            internal NonGeneric SetSingleEntityQueryValuesTo(SortingKey sortingKey, EntityEntry propertiesOwner)
            {
                if (sortingKey == null) throw new ArgumentNullException(nameof(sortingKey));
                ResetVisitState();
                SetNextSortingKey(sortingKey);
                sortingKey.VisitPrefixFirst(UseForSingleEntityQuery(propertiesOwner));
                return this;
            }

            internal NonGeneric UseForSingleEntityQuery(EntityEntry propertiesOwner)
            {
                PropertiesOwner = propertiesOwner ?? throw new ArgumentNullException(nameof(propertiesOwner));
                PropertyOwners = null;
                UseForAllPropertiesSetting();
                return this;
            }

            public NonGeneric SetMultiEntityQueryValuesTo(SortingKey sortingKey, EntityEntry propertiesOwner)
            {
                if (sortingKey == null) throw new ArgumentNullException(nameof(sortingKey));
                ResetVisitState();
                SetNextSortingKey(sortingKey);
                sortingKey.VisitPrefixFirst(UseForMultiEntityQuery(propertiesOwner));
                return this;
            }

            public NonGeneric UseForMultiEntityQuery(EntityEntry propertiesOwner)
            {
                PropertiesOwner = propertiesOwner ?? throw new ArgumentNullException(nameof(propertiesOwner));
                PropertyOwners = null;
                UseForSingleOwnerMatchingPropertiesOnly();
                return this;
            }

            public NonGeneric SetMultiEntityQueryValuesTo(
                SortingKey sortingKey, IReadOnlyList<EntityEntry> propertyOwners)
            {
                if (sortingKey == null) throw new ArgumentNullException(nameof(sortingKey));
                ResetVisitState();
                SetNextSortingKey(sortingKey);
                sortingKey.VisitPrefixFirst(UseForMultiEntityQuery(propertyOwners));
                return this;
            }

            public NonGeneric UseForMultiEntityQuery(IReadOnlyList<EntityEntry> propertyOwners)
            {
                PropertyOwners = propertyOwners ?? throw new ArgumentNullException(nameof(propertyOwners));
                PropertiesOwner = null;
                UseForAllPropertiesSetting();
                return this;
            }

            protected override TProperty GetValue<TProperty>(EntityEntry propertiesOwner, string propertyName)
            {
                if (propertiesOwner == null) throw new ArgumentNullException(nameof(propertiesOwner));
                var entryEntityType = propertiesOwner.Metadata.ClrType ?? throw new ApplicationException("propertiesOwner.Metadata.ClrType == null");
                var getter = EFEntityEntryPropertiesCache<TProperty>.GetGetter(entryEntityType, propertyName);
                var propertyValue = getter(propertiesOwner);
                return propertyValue;
            }

            protected override void SetValue<TProperty>(
                SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName, TProperty value)
            {
                if (PropertiesOwner != null)
                {
                    if (PropertyOwners != null) throw new ApplicationException("PropertiesOwner != null && PropertyOwners != null");
                    SetValue(propertyOwner, propertyName, value, PropertiesOwner);
                }
                else if (PropertyOwners != null)
                {
                    if (_setOnlyMatchingToSingleOwner) throw new ApplicationException("_setOnlyMatchingToSingleOwner");
                    if (!propertyOwner.TryGetSingleMatchingOwner(PropertyOwners, out var entry))
                        throw new ApplicationException("!propertyOwner.TryGetSingleMatchingOwner(PropertyOwners, out var entry)");

                    SetValue(propertyOwner, propertyName, value, entry);
                }
                else
                {
                    throw new ApplicationException("PropertiesOwner == null && PropertyOwners == null");
                }
            }

            private void SetValue<TProperty>(
                SortingKeyTopology.PropertiesOwner propertyOwner,
                string propertyName,
                TProperty value,
                EntityEntry propertiesOwner)
            {
                if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
                if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
                if (propertiesOwner == null) throw new ArgumentNullException(nameof(propertiesOwner));

                if (!CheckToSkip(propertyOwner, propertyName, value, propertiesOwner))
                {
                    var entryEntityType = propertiesOwner.Metadata.ClrType ?? throw new ApplicationException("propertiesOwner.Metadata.ClrType == null");
                    var setter = EFEntityEntryPropertiesCache<TProperty>.GetSetter(entryEntityType, propertyName);
                    setter(propertiesOwner, value);
                }
            }
        }
    }
}