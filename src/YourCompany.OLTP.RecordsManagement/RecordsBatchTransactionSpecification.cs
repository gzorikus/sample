using System;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public abstract partial class RecordsBatchTransactionSpecification
    {
        private RecordsBatchTransactionSpecification() { }

        public virtual bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
            => !ReferenceEquals(this, other);

        public interface ISpecificationWrapper
        {
            Type RecordType { get; }
            Type RecordDataType { get; }
            Exception GetExceptionForSpecifiedRecordsMismatch(Identity identity);
        }

        internal interface ISpecificationWrapper<TRecordData> : ISpecificationWrapper, ISpecification<TRecordData>
            where TRecordData : class
        {
            ISpecification<TRecordData> SpecificationToMatch { get; }
        }

        public abstract class SpecificationToMatchWithoutDataChanges<TRecord, TRecordData>
            : RecordsBatchTransactionSpecification, ISpecificationWrapper<TRecordData>
            where TRecord : class
            where TRecordData : class
        {
            Type ISpecificationWrapper.RecordType => typeof(TRecord);
            Type ISpecificationWrapper.RecordDataType => typeof(TRecordData);

            public abstract ISpecification<TRecordData> SpecificationToMatch { get; }
            protected SpecificationToMatchWithoutDataChanges() { }

            bool ISpecification<TRecordData>.Match(TRecordData recordData)
                => SpecificationToMatch?.Match(recordData) ?? throw new ApplicationException("specification == null");

            public virtual Exception GetExceptionForSpecifiedRecordsMismatch(Identity identity)
                => new RecordDataMismatchException<TRecordData>(identity, SpecificationToMatch);

            public override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                => base.ValidateCompatibilityWith(other)
                && (!(other is ISpecificationWrapper<TRecordData> wrapper)
                    || (!wrapper.SpecificationToMatch.Equals(SpecificationToMatch)
                        && ValidateCompatibilityWith(wrapper.SpecificationToMatch)));

            public virtual bool ValidateCompatibilityWith(ISpecification<TRecordData> otherSpecification) => true;

            public abstract class SelfDeclaring : SpecificationToMatchWithoutDataChanges<TRecord, TRecordData>,
                ISpecification<TRecordData>
            {
                public sealed override ISpecification<TRecordData> SpecificationToMatch => this;
                protected abstract bool Match(TRecordData recordData);
                bool ISpecification<TRecordData>.Match(TRecordData recordData) => Match(recordData);
            }
        }

        public sealed class RecordDataMismatchException<TRecordData> : Exception where TRecordData : class
        {
            public Identity SpecifiedRecordIdentity { get; }
            public ISpecification<TRecordData> SpecifiedRecordMismatchedSpecification { get; }

            internal RecordDataMismatchException(
                Identity specifiedRecordIdentity, ISpecification<TRecordData> specifiedRecordMismatchedSpecification)
            {
                SpecifiedRecordIdentity = specifiedRecordIdentity ?? throw new ArgumentNullException(nameof(specifiedRecordIdentity));
                SpecifiedRecordMismatchedSpecification = specifiedRecordMismatchedSpecification ?? throw new ArgumentNullException(nameof(specifiedRecordMismatchedSpecification));
            }
        }
    }
}