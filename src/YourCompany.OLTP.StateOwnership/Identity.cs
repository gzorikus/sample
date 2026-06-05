using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace YourCompany.OLTP.StateOwnership
{
    public abstract class Identity : IEquatable<Identity>, IComparable<Identity>
    {
        private bool _assigned;

        public sealed override bool Equals(object obj) => obj is Identity identity && Equals(identity);
        public bool Equals(Identity other)
        {
            EnsureWasAssigned();
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.CheckExpectsAssignment()) throw new ApplicationException("other.CheckExpectsAssignment()");
            return EqualsAfterAssignment(other);
        }

        public sealed override int GetHashCode()
        {
            EnsureWasAssigned();
            return GetHashCodeAfterAssignment();
        }

        public int CompareTo(Identity other)
        {
            EnsureWasAssigned();
            if (ReferenceEquals(other, null)) return 1;
            if (ReferenceEquals(this, other)) return 0;
            if (other.CheckExpectsAssignment()) throw new ApplicationException("other.CheckExpectsAssignment()");
            return CompareAfterAssignment(other);
        }

        public ComparisonOptions CompareBy => new ComparisonOptions(this);

        protected void ConfirmAssigned()
        {
            EnsureExpectsAssignment();
            _assigned = true;
        }

        protected void EnsureExpectsAssignment()
        {
            if (!CheckExpectsAssignment()) throw new ApplicationException("!CheckExpectsAssignment()");
        }

        protected void EnsureWasAssigned()
        {
            if (CheckExpectsAssignment()) throw new ApplicationException("CheckExpectsAssignment()");
        }

        public bool CheckExpectsAssignment() => !_assigned;

        protected abstract bool EqualsAfterAssignment(Identity other);
        protected abstract int GetHashCodeAfterAssignment();
        protected abstract int CompareAfterAssignment(Identity other);

        public static bool operator ==(Identity obj1, Identity obj2)
        {
            if (ReferenceEquals(obj1, obj2)) return true;
            if (ReferenceEquals(obj1, null)) return false;
            if (ReferenceEquals(obj2, null)) return false;
            return obj1.Equals(obj2);
        }

        public static bool operator !=(Identity obj1, Identity obj2) => !(obj1 == obj2);

        public static bool operator >(Identity obj1, Identity obj2)
        {
            if (ReferenceEquals(obj1, null)) return false;
            if (ReferenceEquals(obj2, null)) return false;
            return obj1.CompareTo(obj2) > 0;
        }

        public static bool operator <(Identity obj1, Identity obj2)
        {
            if (ReferenceEquals(obj1, null)) return false;
            if (ReferenceEquals(obj2, null)) return false;
            return obj1.CompareTo(obj2) < 0;
        }

        public static bool operator >=(Identity obj1, Identity obj2)
        {
            if (ReferenceEquals(obj1, null)) return false;
            if (ReferenceEquals(obj2, null)) return false;
            return obj1.CompareTo(obj2) >= 0;
        }

        public static bool operator <=(Identity obj1, Identity obj2)
        {
            if (ReferenceEquals(obj1, null)) return false;
            if (ReferenceEquals(obj2, null)) return false;
            return obj1.CompareTo(obj2) <= 0;
        }

        public readonly struct ComparisonOptions
        {
            private Identity Identity { get; }
            internal ComparisonOptions(Identity identity) => Identity = identity;

            public ReferenceOptions Reference => new ReferenceOptions(Identity);
            public ValueOptions Value => new ValueOptions(Identity);

            public readonly struct ReferenceOptions
            {
                private Identity Identity { get; }
                internal ReferenceOptions(Identity identity) => Identity = identity;

                public bool Equals(Identity other) => ReferenceComparer.AssignmentAgnostic.Instance.Equals(Identity, other);
                public new int GetHashCode() => ReferenceComparer.AssignmentAgnostic.Instance.GetHashCode(Identity);

                public AssignedOnlyOptions AssignedOnly => new AssignedOnlyOptions(Identity);

                public readonly struct AssignedOnlyOptions
                {
                    private Identity Identity { get; }
                    internal AssignedOnlyOptions(Identity identity) => Identity = identity;

                    public bool Equals(Identity other) => ReferenceComparer.AssignmentEnsuring.Instance.Equals(Identity, other);
                    public new int GetHashCode() => ReferenceComparer.AssignmentEnsuring.Instance.GetHashCode(Identity);
                }
            }

            public readonly struct ValueOptions
            {
                private Identity Identity { get; }
                internal ValueOptions(Identity identity) => Identity = identity;

                public AssignedOnlyOptions AssignedOnly => new AssignedOnlyOptions(Identity);

                public readonly struct AssignedOnlyOptions
                {
                    private Identity Identity { get; }
                    internal AssignedOnlyOptions(Identity identity) => Identity = identity;
                    public bool Equals(Identity other) => Identity == other;
                    public new int GetHashCode() => Identity.GetHashCode();
                    public int CompareTo(Identity other) => Identity.CompareTo(other);
                }
            }
        }

        public abstract class ReferenceComparer : IEqualityComparer<Identity>
        {
            private ReferenceComparer() { }

            bool IEqualityComparer<Identity>.Equals(Identity x, Identity y) => throw new NotImplementedException();
            int IEqualityComparer<Identity>.GetHashCode(Identity obj) => throw new NotImplementedException();

            public sealed class AssignmentAgnostic : ReferenceComparer, IEqualityComparer<Identity>
            {
                public static AssignmentAgnostic Instance { get; } = new AssignmentAgnostic();
                public bool Equals(Identity x, Identity y) => ReferenceEquals(x, y);
                public int GetHashCode(Identity obj) => RuntimeHelpers.GetHashCode(obj);
            }

            public sealed class AssignmentEnsuring : ReferenceComparer, IEqualityComparer<Identity>
            {
                public static AssignmentEnsuring Instance { get; } = new AssignmentEnsuring();

                public bool Equals(Identity x, Identity y)
                {
                    if (!ReferenceEquals(x, null) && x.CheckExpectsAssignment()) throw new ApplicationException("x.CheckExpectsAssignment()");
                    if (!ReferenceEquals(y, null) && y.CheckExpectsAssignment()) throw new ApplicationException("y.CheckExpectsAssignment()");
                    return ReferenceEquals(x, y);
                }

                public int GetHashCode(Identity obj)
                {
                    obj.EnsureWasAssigned();
                    return RuntimeHelpers.GetHashCode(obj);
                }
            }
        }
    }
}