using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using YourCompany.Configuration.EFCore.ChangeTracking;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking
{
    public abstract partial class EntityEntryPropertiesSettingVisitor
    {
        public class NonGeneric : EntityEntryPropertiesSettingVisitor
        {
            public EntityEntry PropertiesOwner { get; private set; }

            public NonGeneric SetSingleEntityQueryValuesTo(SortingKey sortingKey, EntityEntry propertiesOwner)
            {
                if (sortingKey == null) throw new ArgumentNullException(nameof(sortingKey));
                ResetVisitState();
                SetNextSortingKey(sortingKey);
                sortingKey.VisitPrefixFirst(UseForSingleEntityQuery(propertiesOwner));
                return this;
            }

            public NonGeneric UseForSingleEntityQuery(EntityEntry propertiesOwner)
            {
                PropertiesOwner = propertiesOwner ?? throw new ArgumentNullException(nameof(propertiesOwner));
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
                => SetValue(propertyOwner, propertyName, value, PropertiesOwner);

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