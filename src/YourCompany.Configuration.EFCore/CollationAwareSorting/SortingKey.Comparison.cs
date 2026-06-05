using System;

namespace YourCompany.Configuration.EFCore.CollationAwareSorting
{
    public abstract partial class SortingKey
    {
        public sealed override bool Equals(object obj) => obj is SortingKey other && Equals(other);
        public bool Equals(SortingKey other)
        {
            if (PropertyName == null) throw new ApplicationException("PropertyName == null");
            if (other != null && other.PropertyName == null) throw new ApplicationException("other != null && other.PropertyName == null");
            return SortingKeyTopology.Comparer.Instance.Equals(this, other) && CheckMatchingTopologyValuesEqualRecursive(other);
        }

        public sealed override int GetHashCode()
        {
            if (PropertyName == null) throw new ApplicationException("PropertyName == null");
            return HashCode.Combine(SortingKeyTopology.Comparer.Instance.GetHashCode(this), GetValuesHashCodeRecursive());
        }

        public int CompareTo(SortingKey other)
        {
            if (PropertyName == null) throw new ApplicationException("PropertyName == null");
            if (ReferenceEquals(other, null)) return 1;
            if (ReferenceEquals(this, other)) return 0;
            if (other.PropertyName == null) throw new ApplicationException("other.PropertyName == null");
            if (!SortingKeyTopology.Comparer.Instance.Equals(other)) throw new ApplicationException("!KeyTopologyComparer.Instance.Equals(other)");
            return CompareMatchingTopologyValuesRecursivePrefixFirst(other);
        }

        public static bool operator ==(SortingKey obj1, SortingKey obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (ReferenceEquals(obj1, null)) return false;
            if (ReferenceEquals(obj2, null)) return false;
            return obj1.Equals(obj2);
        }

        public static bool operator !=(SortingKey obj1, SortingKey obj2) => !(obj1 == obj2);

        public static bool operator >(SortingKey obj1, SortingKey obj2)
        {
            if (ReferenceEquals(obj1, null)) return false;
            if (ReferenceEquals(obj2, null)) return false;
            return obj1.CompareTo(obj2) > 0;
        }

        public static bool operator <(SortingKey obj1, SortingKey obj2)
        {
            if (ReferenceEquals(obj1, null)) return false;
            if (ReferenceEquals(obj2, null)) return false;
            return obj1.CompareTo(obj2) < 0;
        }

        public static bool operator >=(SortingKey obj1, SortingKey obj2)
        {
            if (ReferenceEquals(obj1, null)) return false;
            if (ReferenceEquals(obj2, null)) return false;
            return obj1.CompareTo(obj2) >= 0;
        }

        public static bool operator <=(SortingKey obj1, SortingKey obj2)
        {
            if (ReferenceEquals(obj1, null)) return false;
            if (ReferenceEquals(obj2, null)) return false;
            return obj1.CompareTo(obj2) <= 0;
        }
    }
}