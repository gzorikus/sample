using System;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting.ChangeTracking
{
    internal abstract partial class EntityEntryPropertiesSettingVisitor : SortingKeyPropertiesVisitor
    {
        private bool _setOnlyMatchingToSingleOwner;
        private SortingKeyTopology.PropertiesOwner? _singleMatchingPropertiesOwner;

        protected void UseForAllPropertiesSetting()
        {
            ModelProvider = null;
            _setOnlyMatchingToSingleOwner = false;
            _singleMatchingPropertiesOwner = null;
        }

        protected void UseForSingleOwnerMatchingPropertiesOnly()
        {
            ModelProvider = null;
            _setOnlyMatchingToSingleOwner = true;
            _singleMatchingPropertiesOwner = null;
        }

        protected override void VisitCurrentProperty<TProperty>(SortingKey property, TProperty value)
        {
            base.VisitCurrentProperty(property, value);
            if (!CheckToSkip(property, value)) SetValue(property.PropertyOwner, property.PropertyName, value);
        }

        protected virtual bool CheckToSkip<TProperty>(SortingKey property, TProperty value) => false;

        protected virtual bool CheckToSkip<TProperty>(
            SortingKeyTopology.PropertiesOwner propertyOwner,
            string propertyName,
            TProperty value,
            EntityEntry propertiesOwner)
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (propertiesOwner == null) throw new ArgumentNullException(nameof(propertiesOwner));

            if (_setOnlyMatchingToSingleOwner)
            {
                if (!propertyOwner.Match(propertiesOwner.Metadata)) return true;

                _singleMatchingPropertiesOwner ??= propertyOwner;
                if (!propertyOwner.MatchSingleTopology(_singleMatchingPropertiesOwner.Value))
                    throw new ApplicationException("!propertyOwner.MatchSingleTopology(_singleMatchingPropertiesOwner.Value)");
            }

            var propertyEntry = propertiesOwner.Property(propertyName) ?? throw new ApplicationException("propertiesOwner.Property(propertyName) == null");
            if (!propertyEntry.IsModified) return false;
            var propertyValue = GetValue<TProperty>(propertiesOwner, propertyName);

            var modelProvider = ModelProvider ?? throw new ApplicationException("ModelProvider == null");
            var equalityComparer = modelProvider.GetModelEqualityComparerRequired<TProperty>(propertyOwner, propertyName);
            if (!equalityComparer.Equals(value, propertyValue))
                throw new ApplicationException("propertyEntry.IsModified && !equalityComparer.Equals(value, propertyValue)");

            return true;
        }

        protected abstract TProperty GetValue<TProperty>(EntityEntry propertiesOwner, string propertyName);

        protected abstract void SetValue<TProperty>(
            SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName, TProperty value);
    }
}