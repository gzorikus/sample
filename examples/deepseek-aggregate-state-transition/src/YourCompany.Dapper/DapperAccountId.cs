using System;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.Dapper
{
    internal sealed class DapperAccountId : Identity
    {
        private long? _value;
        private Guid? _publicId;

        internal DapperAccountId() { }
        internal DapperAccountId(long value) => _value = value;
        internal DapperAccountId(Guid publicId) => _publicId = publicId;

        internal long Value
        {
            get => _value ?? throw new ApplicationException("!_value.HasValue");
            set
            {
                if (_value.HasValue) throw new ApplicationException("_value.HasValue");
                _value = value;
                if (_publicId.HasValue) ConfirmAssigned();
            }
        }

        internal Guid PublicId
        {
            get => _publicId ?? throw new ApplicationException("!_publicId.HasValue");
            set
            {
                if (_publicId.HasValue) throw new ApplicationException("_publicId.HasValue");
                _publicId = value;
                if (_value.HasValue) ConfirmAssigned();
            }
        }

        internal bool HasValue => _value.HasValue;
        internal bool HasPublicId => _publicId.HasValue;

        protected override bool EqualsAfterAssignment(Identity other) =>
            other is DapperAccountId otherId && Value.Equals(otherId.Value);

        protected override int GetHashCodeAfterAssignment() => Value.GetHashCode();

        protected override int CompareAfterAssignment(Identity other) =>
            other is DapperAccountId otherId
                ? Value.CompareTo(otherId.Value)
                : throw new ArgumentOutOfRangeException(nameof(other), other, message: null);
    }
}