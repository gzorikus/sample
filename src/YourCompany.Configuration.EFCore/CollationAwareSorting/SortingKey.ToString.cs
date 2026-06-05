using System;
using System.Collections.Generic;
using System.Text;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public abstract partial class SortingKey
    {
        public sealed override string ToString()
            => ToStringRecursive(GetToStringReplacingQueriedSetNamePrefix(), PropertyOwner, restPropertyOwners: null);

        protected abstract bool CheckMatchingTopologyValuesEqualRecursive(SortingKey other);
        protected abstract int GetValuesHashCodeRecursive();
        protected abstract int CompareMatchingTopologyValuesRecursivePrefixFirst(SortingKey other);
        protected abstract string ToStringValue();

        internal string ToStringWithoutQuery()
            => ToStringRecursive(prefix: "", singleOrFirstPropertiesOwner: null, restPropertyOwners: null);

        internal string ToStringRecursive(
            string prefix,
            SortingKeyTopology.PropertiesOwner? singleOrFirstPropertiesOwner,
            HashSet<SortingKeyTopology.PropertiesOwner> restPropertyOwners)
        {
            if (PrefixKey == null)
            {
                prefix ??= GetToStringPropertyOwnersPrefixStringBuilder(
                    ModelProvider, singleOrFirstPropertiesOwner, restPropertyOwners).ToString();
            }
            else
            {
                if (prefix == null && PropertyName != null)
                {
                    singleOrFirstPropertiesOwner ??= PropertyOwner;
                    if (PropertyOwner != singleOrFirstPropertiesOwner)
                    {
                        restPropertyOwners ??= new HashSet<SortingKeyTopology.PropertiesOwner>();
                        restPropertyOwners.Add(PropertyOwner);
                    }
                }
                prefix = PrefixKey.ToStringRecursive(prefix, singleOrFirstPropertiesOwner, restPropertyOwners);
            }

            return $"{prefix} {PropertyName ?? "N/A"}{(Descending ? "↓" : "↑")}{ToStringValue()})";
        }

        internal string GetToStringReplacingQueriedSetNamePrefix()
            => ReplaceQueriedSetName != null ? $"[{ReplaceQueriedSetName}]" : null;

        internal static StringBuilder GetToStringReplacingQueriedSetNamePrefixStringBuilder(
            string replacingQueriedSetName, int extraCapacity)
        {
            if (replacingQueriedSetName == null) throw new ArgumentNullException(nameof(replacingQueriedSetName));
            var sb = new StringBuilder(33 + extraCapacity);
            sb.Append('[');
            sb.Append(replacingQueriedSetName);
            sb.Append(']');
            return sb;
        }

        internal static StringBuilder GetToStringPropertyOwnersPrefixStringBuilder(
            ICollationAwareModelProvider modelProvider,
            SortingKeyTopology.PropertiesOwner? singleOrFirstPropertiesOwner,
            HashSet<SortingKeyTopology.PropertiesOwner> restPropertyOwners,
            int extraCapacity = 0)
        {
            if (restPropertyOwners != null && singleOrFirstPropertiesOwner == null) throw new ApplicationException("restPropertyOwners != null && singleOrFirstPropertiesOwner == null");

            int propertyOwnersCount = restPropertyOwners?.Count + 1 ?? 1;
            var sb = new StringBuilder(propertyOwnersCount * 33 + extraCapacity);

            sb.Append('[');
            if (modelProvider != null && singleOrFirstPropertiesOwner.HasValue)
            {
                sb.Append(modelProvider.GetEntityTableName(singleOrFirstPropertiesOwner.Value));
                if (restPropertyOwners != null)
                {
                    foreach (var propertiesOwner in restPropertyOwners)
                    {
                        sb.Append('|');
                        sb.Append(modelProvider.GetEntityTableName(propertiesOwner));
                    }
                }
            }
            else
            {
                sb.Append("N/A");
            }
            sb.Append(']');

            return sb;
        }
    }
}