using System;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public abstract partial class SortingKey : SortingKeyTopology.ILastProperty
    {
        internal static string ValuePropertyName
            => nameof(ValueHolding<int>.Value) == nameof(ValueHolding<int>.ConvertedFrom<int>.Value)
                ? nameof(ValueHolding<int>.Value)
                : throw new ApplicationException("nameof(SortingKey.ValueHolding<int>.Value) != nameof(SortingKey.ValueHolding<int>.ConvertedFrom<int>.Value)");

        public ICollationAwareModelProvider ModelProvider { get; }
        SortingKeyTopology.ILastProperty SortingKeyTopology.ILastProperty.Prefix => PrefixKey;
        public SortingKey PrefixKey { get; }
        public int PrefixKeysCount { get; }
        public bool MultiplePropertyOwners { get; private set; }
        public bool UniquePropertyNames { get; private set; }
        public SortingKeyTopology.PropertiesOwner PropertyOwner { get; private set; }
        public string PropertyName { get; private set; }
        public abstract Type PropertyValueType { get; }
        public bool Descending { get; private set; }

        private SortingKey(ICollationAwareModelProvider modelProvider, SortingKey prefixKey)
        {
            ModelProvider = modelProvider ?? throw new ArgumentNullException(nameof(modelProvider));

            if (prefixKey != null)
            {
                if (prefixKey.PropertyName == null) throw new ApplicationException("prefixKey.PropertyName == null");
                if (prefixKey.ReplaceQueriedSetName != null) throw new ApplicationException("prefixKey.ReplaceQueriedSetName != null");
                if (modelProvider != prefixKey.ModelProvider) throw new ApplicationException("modelProvider != prefixKey.ModelProvider");
                PrefixKey = prefixKey;
                PrefixKeysCount = prefixKey.PrefixKeysCount + 1;
            }
        }

        private void SetProperty(SortingKeyTopology.PropertiesOwner propertyOwner, string propertyName, bool descending)
        {
            if (propertyOwner == default) throw new ArgumentNullException(nameof(propertyOwner));
            if (propertyName == null) throw new ArgumentNullException(nameof(propertyName));
            if (PropertyValueType != ModelProvider.GetPropertyValueType(propertyOwner, propertyName)) throw new ApplicationException("PropertyValueType != ModelProvider.GetPropertyValueType(propertyOwner, propertyName)");
            if (PropertyName != null) throw new ApplicationException("PropertyName != null");
            if (MultiplePropertyOwners || UniquePropertyNames) throw new ApplicationException("MultiplePropertyOwners || UniquePropertyNames");
            if (ReplaceQueriedSetName != null) throw new ApplicationException("ReplaceQueriedSetName != null");

            var prefixKey = PrefixKey;
            if (prefixKey != null)
                MultiplePropertyOwners
                    = prefixKey.MultiplePropertyOwners || !propertyOwner.MatchSingleTopology(prefixKey.PropertyOwner);

            UniquePropertyNames = true;
            while (UniquePropertyNames && prefixKey != null)
            {
                UniquePropertyNames = prefixKey.UniquePropertyNames && prefixKey.PropertyName != propertyName;
                prefixKey = prefixKey.PrefixKey;
            }

            PropertyOwner = propertyOwner;
            PropertyName = propertyName;
            Descending = descending;

            if (propertyOwner.IsSharedEntityType && !MultiplePropertyOwners)
                ReplaceQueriedSetName = propertyOwner.EntityName;
        }
    }
}