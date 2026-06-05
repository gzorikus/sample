using System.Collections.Generic;
using System.Text;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    internal sealed partial class OrElseEqualityKeys
    {
        public override string ToString()
        {
            if (Count == 0) return "Count == 0";

            var singleOrFirstPropertiesOwner = this[0]?.PropertyOwner;
            HashSet<SortingKeyTopology.PropertiesOwner> restPropertyOwners = null;
            var sb = new StringBuilder();

            sb.Append(this[0]?.ToStringWithoutQuery() ?? "N/A");
            for (int i = 1; i < Count; i++)
            {
                var item = this[i];

                var propertyOwner = item?.PropertyOwner;
                singleOrFirstPropertiesOwner ??= propertyOwner;
                if (ReplaceQueriedSetName == null && propertyOwner.HasValue && propertyOwner != singleOrFirstPropertiesOwner)
                {
                    restPropertyOwners ??= new HashSet<SortingKeyTopology.PropertiesOwner>();
                    restPropertyOwners.Add(propertyOwner.Value);
                }

                sb.Append(" OR ");
                sb.Append(item?.ToStringWithoutQuery() ?? "N/A");
            }

            var prefixSb = ReplaceQueriedSetName != null
                ? SortingKey.GetToStringReplacingQueriedSetNamePrefixStringBuilder(
                    ReplaceQueriedSetName, extraCapacity: sb.Capacity)
                : SortingKey.GetToStringPropertyOwnersPrefixStringBuilder(
                    ModelProvider, singleOrFirstPropertiesOwner, restPropertyOwners, extraCapacity: sb.Capacity);
            prefixSb.Append(sb);
            sb = prefixSb;

            return sb.ToString();
        }
    }
}