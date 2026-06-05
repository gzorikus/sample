using System;

namespace YourCompany.OLTP.RecordsManagement.Persistence
{
    public abstract partial class UniqueKey
    {
        public static class Alternate
        {
            public abstract class Sorting : UniqueKey
            {
                public bool CanBeResolvedIntoPrivatePrimaryKey => this is INaturalPublicCursor;
                public bool PrefixedPrivatePrimaryKeySortingRequired => this is IPrivatePrimaryKeyPrefix;

                public bool PrefixedPrivatePrimaryKeySortingDescending
                    => this is IPrivatePrimaryKeyPrefix prefixed && prefixed.PrivatePrimaryKeySortingDescending;

                protected Sorting()
                {
                    if (this is INaturalPublicCursor && this is IPrivatePrimaryKeyPrefix) throw new ApplicationException("this is INaturalPublicCursor && this is IPrivatePrimaryKeyPrefix");
                    if (!(this is INaturalPublicCursor) && !(this is IPrivatePrimaryKeyPrefix)) throw new ApplicationException("!(this is INaturalPublicCursor) && !(this is IPrivatePrimaryKeyPrefix)");
                }

                public T ImplementedWith<T>() where T : Sorting
                {
                    if (typeof(T) == typeof(Sorting)) throw new ApplicationException("typeof(T) == typeof(Sorting)");
                    return this as T ?? throw new ApplicationException("this is not T");
                }

                protected abstract bool EqualsAfterAssignmentWithSameSortingType(Sorting other);
                protected sealed override bool EqualsAfterAssignment(UniqueKey other)
                {
                    if (!CanBeResolvedIntoPrivatePrimaryKey) throw new ApplicationException("!CanBeResolvedIntoPrivatePrimaryKey");
                    var otherSorting = other as Sorting;
                    return otherSorting?.GetType() == GetType() && EqualsAfterAssignmentWithSameSortingType(otherSorting);
                }

                protected abstract int CompareAfterAssignmentWithSameSortingType(Sorting other);
                protected sealed override int CompareAfterAssignment(UniqueKey other)
                {
                    var otherSorting = other.AsAssignedBeforeSorting()
                        ?? other.AsAssignedAfterSorting()?.AlternateSorting
                            ?? throw new ApplicationException("otherSorting == null");
                    return otherSorting.GetType() == GetType()
                        ? CompareAfterAssignmentWithSameSortingType(otherSorting)
                        : throw new ApplicationException("otherSorting.GetType() != GetType()");
                }

                public override string ToString() => CanBeResolvedIntoPrivatePrimaryKey ? "(Natural)" : "(Prefix)";

                public interface INaturalPublicCursor
                {
                }

                public interface IPrivatePrimaryKeyPrefix
                {
                    bool PrivatePrimaryKeySortingDescending { get; }
                }
            }
        }
    }
}