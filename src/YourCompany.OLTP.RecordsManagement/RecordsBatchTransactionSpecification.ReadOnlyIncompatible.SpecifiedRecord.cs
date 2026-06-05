using System;
using System.Linq;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public abstract partial class RecordsBatchTransactionSpecification
    {
        public abstract partial class ReadOnlyIncompatible
        {
            public abstract class SpecifiedRecord : ReadOnlyIncompatible
            {
                public Identity Identity { get; }
                internal SpecifiedRecord(Identity identity) => Identity = identity ?? throw new ArgumentNullException(nameof(identity));

                internal override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                    => base.ValidateCompatibilityWith(other)
                    && !(other is SpecifiedRecordIncompatible)
                    && !(other is Sorting.AfterId)
                    && (!(other is Sorting.ByIds byIds)
                        || byIds.OrderedIdentities.Contains(Identity, Identity.ReferenceComparer.AssignmentAgnostic.Instance));

                public new sealed class UseCaseParameters : SpecifiedRecord
                {
                    public EventArgs UseCaseParametersToTrigger { get; }

                    internal UseCaseParameters(Identity identity, EventArgs useCaseParametersToTrigger) : base(identity)
                        => UseCaseParametersToTrigger = useCaseParametersToTrigger ?? throw new ArgumentNullException(nameof(useCaseParametersToTrigger));

                    internal override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                        => base.ValidateCompatibilityWith(other)
                        && !(other is ReadOnlyIncompatible.UseCaseParameters)
                        && (!(other is UseCaseParameters useCaseParameters)
                            || !useCaseParameters.Identity.CompareBy.Reference.Equals(Identity))
                        && (!(other is ReadOnlyIncompatible)
                            || !(UseCaseParametersToTrigger is TransactionCallback
                                .ForStateAssertion
                                .TriggeredBeforeDataChanging)
                            || (other is SpecifiedRecord specifiedRecord
                                && !specifiedRecord.Identity.CompareBy.Reference.Equals(Identity)));
                }

                public new abstract class SpecificationToMatchAfterDataChanging<TRecord, TRecordData> : SpecifiedRecord,
                    ISpecificationWrapper<TRecordData>
                    where TRecord : class
                    where TRecordData : class
                {
                    Type RecordsBatchTransactionSpecification.ISpecificationWrapper.RecordType => typeof(TRecord);
                    Type RecordsBatchTransactionSpecification.ISpecificationWrapper.RecordDataType => typeof(TRecordData);
                    Identity ISpecificationWrapper.SpecifiedRecordIdentity => Identity;

                    public abstract ISpecification<TRecordData> SpecificationToMatch { get; }
                    protected SpecificationToMatchAfterDataChanging(Identity identity) : base(identity) { }

                    bool ISpecification<TRecordData>.Match(TRecordData recordData)
                        => SpecificationToMatch?.Match(recordData) ?? throw new ApplicationException("specification == null");

                    public virtual Exception GetExceptionForSpecifiedRecordsMismatch(Identity identity)
                        => new RecordDataMismatchException<TRecordData>(identity, SpecificationToMatch);

                    internal override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                        => base.ValidateCompatibilityWith(other)
                        && (!(other is ISpecificationWrapper<TRecordData> wrapper)
                            || (!wrapper.SpecificationToMatch.Equals(SpecificationToMatch)
                                && ValidateCompatibilityWith(wrapper.SpecificationToMatch)
                                && other is SpecifiedRecord specifiedRecord
                                && !specifiedRecord.Identity.CompareBy.Reference.Equals(Identity)));

                    public virtual bool ValidateCompatibilityWith(ISpecification<TRecordData> otherSpecification) => true;

                    public abstract class SelfDeclaring : SpecificationToMatchAfterDataChanging<TRecord, TRecordData>,
                        ISpecification<TRecordData>
                    {
                        protected SelfDeclaring(Identity identity) : base(identity) { }
                        public sealed override ISpecification<TRecordData> SpecificationToMatch => this;
                        protected abstract bool Match(TRecordData recordData);
                        bool ISpecification<TRecordData>.Match(TRecordData recordData) => Match(recordData);
                    }
                }
            }
        }
    }
}