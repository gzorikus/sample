using System;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public abstract partial class RecordsBatchTransactionSpecification
    {
        public abstract partial class ReadOnlyIncompatible : RecordsBatchTransactionSpecification
        {
            internal ReadOnlyIncompatible() { }

            internal override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                => base.ValidateCompatibilityWith(other)
                && !(other is ReadOnly)
                && (!(other is UseCaseParameters eachRecordUseCaseParameters)
                    || !(eachRecordUseCaseParameters.UseCaseParametersToTrigger is TransactionCallback
                        .ForStateAssertion
                        .TriggeredBeforeDataChanging))
                && (!(other is SpecifiedRecord.UseCaseParameters specifiedRecordUseCaseParameters)
                    || !(specifiedRecordUseCaseParameters.UseCaseParametersToTrigger is TransactionCallback
                        .ForStateAssertion
                        .TriggeredBeforeDataChanging)
                    || (this is SpecifiedRecord specifiedRecord
                        && !specifiedRecord.Identity.CompareBy.Reference.Equals(specifiedRecordUseCaseParameters.Identity)));

            internal new interface ISpecificationWrapper : RecordsBatchTransactionSpecification.ISpecificationWrapper
            {
                Identity SpecifiedRecordIdentity { get; }
            }

            internal new interface ISpecificationWrapper<TRecordData> : ISpecificationWrapper,
                RecordsBatchTransactionSpecification.ISpecificationWrapper<TRecordData>
                where TRecordData : class
            { }

            public sealed class UseCaseParameters : ReadOnlyIncompatible
            {
                internal static UseCaseParameters MatchRecordsWithoutUseCaseChanges
                    = new UseCaseParameters(TransactionCallback.ForStateAssertion.TriggeredBeforeDataChanging.Default);

                public EventArgs UseCaseParametersToTrigger { get; }

                internal UseCaseParameters(EventArgs useCaseParametersToTrigger)
                    => UseCaseParametersToTrigger = useCaseParametersToTrigger ?? throw new ArgumentNullException(nameof(useCaseParametersToTrigger));

                internal override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                    => base.ValidateCompatibilityWith(other)
                    && !(other is UseCaseParameters)
                    && !(other is SpecifiedRecord.UseCaseParameters)
                    && (!(other is ReadOnlyIncompatible)
                        || !(UseCaseParametersToTrigger is TransactionCallback.ForStateAssertion.TriggeredBeforeDataChanging));
            }

            public abstract class SpecificationToMatchAfterDataChanging<TRecord, TRecordData> : ReadOnlyIncompatible,
                ISpecificationWrapper<TRecordData>
                where TRecord : class
                where TRecordData : class
            {
                Type RecordsBatchTransactionSpecification.ISpecificationWrapper.RecordType => typeof(TRecord);
                Type RecordsBatchTransactionSpecification.ISpecificationWrapper.RecordDataType => typeof(TRecordData);
                Identity ISpecificationWrapper.SpecifiedRecordIdentity => null;

                public abstract ISpecification<TRecordData> SpecificationToMatch { get; }
                protected SpecificationToMatchAfterDataChanging() { }

                bool ISpecification<TRecordData>.Match(TRecordData recordData)
                    => SpecificationToMatch?.Match(recordData) ?? throw new ApplicationException("specification == null");

                public virtual Exception GetExceptionForSpecifiedRecordsMismatch(Identity identity)
                    => new RecordDataMismatchException<TRecordData>(identity, SpecificationToMatch);

                internal override bool ValidateCompatibilityWith(RecordsBatchTransactionSpecification other)
                    => base.ValidateCompatibilityWith(other)
                    && (!(other is ISpecificationWrapper<TRecordData> wrapper)
                        || (!wrapper.SpecificationToMatch.Equals(SpecificationToMatch)
                            && ValidateCompatibilityWith(wrapper.SpecificationToMatch)
                            && !(other is SpecifiedRecord)));

                public virtual bool ValidateCompatibilityWith(ISpecification<TRecordData> otherSpecification) => true;

                public abstract class SelfDeclaring : SpecificationToMatchAfterDataChanging<TRecord, TRecordData>, ISpecification<TRecordData>
                {
                    public sealed override ISpecification<TRecordData> SpecificationToMatch => this;
                    protected abstract bool Match(TRecordData recordData);
                    bool ISpecification<TRecordData>.Match(TRecordData recordData) => Match(recordData);
                }
            }
        }
    }
}