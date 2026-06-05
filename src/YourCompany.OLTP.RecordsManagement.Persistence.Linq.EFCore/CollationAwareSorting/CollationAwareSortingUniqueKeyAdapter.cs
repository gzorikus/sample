using System;
using YourCompany.Configuration.EFCore.CollationAwareSorting;
using YourCompany.OLTP.StateOwnership.Reflection.EFCore;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore.CollationAwareSorting
{
    public abstract class CollationAwareSortingUniqueKeyAdapter : UniqueKey.Alternate.Sorting
    {
        public SortingKey SortingKey { get; }

        private CollationAwareSortingUniqueKeyAdapter(SortingKey sortingKey)
        {
            SortingKey = sortingKey ?? throw new ArgumentNullException(nameof(sortingKey));
            while (sortingKey != null)
            {
                if (sortingKey.PropertyName == MetadataHelper.RecordDataNeverGeneratedBigintPrimaryKeyIdPropertyName)
                    throw new ApplicationException("sortingKey.PropertyName == MetadataHelper.RecordDataNeverGeneratedBigintPrimaryKeyIdPropertyName");
                sortingKey = sortingKey.PrefixKey;
            }
            ConfirmAssigned();
        }

        protected override bool EqualsAfterAssignmentWithSameSortingType(Alternate.Sorting other)
        {
            if (other is not CollationAwareSortingUniqueKeyAdapter otherAdapter)
                throw new ApplicationException("other is not CollationAwareSortingUniqueKeyAdapter otherAdapter");
            return SortingKey == otherAdapter.SortingKey;
        }

        protected override int GetHashCodeAfterAssignment() => SortingKey.GetHashCode();

        protected override int CompareAfterAssignmentWithSameSortingType(Alternate.Sorting other)
        {
            if (other is not CollationAwareSortingUniqueKeyAdapter otherAdapter)
                throw new ApplicationException("other is not CollationAwareSortingUniqueKeyAdapter otherAdapter");
            return SortingKey.CompareTo(otherAdapter.SortingKey);
        }

        public override string ToString() => base.ToString() + SortingKey;

        public sealed class NaturalPublicCursor : CollationAwareSortingUniqueKeyAdapter, INaturalPublicCursor
        {
            public NaturalPublicCursor(SortingKey uniqueKey) : base(uniqueKey) { }
        }

        public sealed class PrivatePrimaryKeyPrefix : CollationAwareSortingUniqueKeyAdapter, IPrivatePrimaryKeyPrefix
        {
            public bool PrivatePrimaryKeySortingDescending { get; init; }

            public PrivatePrimaryKeyPrefix(SortingKey prefixKey) : base(prefixKey) { }

            protected override bool EqualsAfterAssignmentWithSameSortingType(Alternate.Sorting other)
            {
                if (other is not PrivatePrimaryKeyPrefix otherPrefix) return false;
                return base.EqualsAfterAssignmentWithSameSortingType(other)
                    && PrivatePrimaryKeySortingDescending == otherPrefix.PrivatePrimaryKeySortingDescending;
            }

            protected override int GetHashCodeAfterAssignment()
                => HashCode.Combine(base.GetHashCodeAfterAssignment(), PrivatePrimaryKeySortingDescending);

            protected override int CompareAfterAssignmentWithSameSortingType(Alternate.Sorting other)
            {
                if (other is not PrivatePrimaryKeyPrefix otherPrefix)
                    throw new ApplicationException("other is not PrivatePrimaryKeyPrefix otherPrefix");
                if (PrivatePrimaryKeySortingDescending != otherPrefix.PrivatePrimaryKeySortingDescending)
                    throw new ApplicationException("PrivatePrimaryKeySortingDescending != otherPrefix.PrivatePrimaryKeySortingDescending");
                return base.CompareAfterAssignmentWithSameSortingType(other);
            }

            public override string ToString() => $"{base.ToString()} PK{(PrivatePrimaryKeySortingDescending ? "↓" : "↑")}";
        }
    }
}