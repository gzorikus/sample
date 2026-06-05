using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransactionConfiguration<TRecord>
    {
        public readonly struct ByIds
        {
            internal RecordsBatchTransaction.ISpecifiedRun<TRecord> Transaction { get; }
            internal Identity Single { get; }
            internal HashSet<Identity> UniqueByReference { get; }

            internal ByIds(
                RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction,
                Identity single = null,
                HashSet<Identity> unique = null)
            {
                if (unique != null && unique.Comparer != Identity.ReferenceComparer.AssignmentAgnostic.Instance)
                    throw new ApplicationException("unique != null && unique.Comparer != Identity.ReferenceComparer.AssignmentAgnostic.Instance");
                Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));
                Single = single;
                UniqueByReference = unique;
            }

            public Task<IRecordsBatch<TRecord>> Run(CancellationToken cancellationToken)
            {
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                return transaction.Run(cancellationToken);
            }

            public ByIds MustMatch<TRecordData>(
                RecordsBatchTransactionSpecification
                    .SpecificationToMatchWithoutDataChanges<TRecord, TRecordData> specification)
                where TRecordData : class
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                transaction.For(specification);
                return this;
            }

            public ReadOnly ReadRecordsOnly()
            {
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                transaction.For(RecordsBatchTransactionSpecification.ReadOnly.IncludeRecords.Instance);
                return new ReadOnly(transaction);
            }

            public ReadOnly ReadAndMatchExclusive()
            {
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                transaction.For(RecordsBatchTransactionSpecification
                    .ReadOnlyIncompatible
                    .UseCaseParameters
                    .MatchRecordsWithoutUseCaseChanges);
                return new ReadOnly(transaction);
            }

            public ReadOnly ReadAndMatchExclusive(Identity identity)
            {
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                if (!CheckIsSpecified(identity)) throw new ApplicationException("!CheckIsSpecified(identity)");
                transaction.For(new RecordsBatchTransactionSpecification
                    .ReadOnlyIncompatible
                    .SpecifiedRecord
                    .UseCaseParameters(
                        identity, TransactionCallback.ForStateAssertion.TriggeredBeforeDataChanging.Default));
                return new ReadOnly(transaction);
            }

            public SpecifiedRecordsModifying TriggerUseCase(EventArgs useCaseParameters, Identity identity)
            {
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                if (!CheckIsSpecified(identity)) throw new ApplicationException("!CheckIsSpecified(identity)");
                transaction.For(new RecordsBatchTransactionSpecification
                    .ReadOnlyIncompatible
                    .SpecifiedRecord
                    .UseCaseParameters(identity, useCaseParameters));
                return new SpecifiedRecordsModifying(this);
            }

            public SpecifiedRecordsModifying ChangeToMatch<TRecordData>(
                RecordsBatchTransactionSpecification
                    .ReadOnlyIncompatible
                    .SpecifiedRecord
                    .SpecificationToMatchAfterDataChanging<TRecord, TRecordData> specification)
                where TRecordData : class
                => new SpecifiedRecordsModifying(this).ChangeToMatch(specification);

            private bool CheckIsSpecified(Identity identity)
                => UniqueByReference != null && UniqueByReference.Contains(identity)
                || Single.CompareBy.Reference.Equals(identity);

            public readonly struct SpecifiedRecordsModifying
            {
                public ByIds Parent { get; }
                internal SpecifiedRecordsModifying(ByIds parent) => Parent = parent;

                public Task<IRecordsBatch<TRecord>> Run(CancellationToken cancellationToken)
                {
                    var transaction = Parent.Transaction ?? throw new ApplicationException("transaction == null");
                    return transaction.Run(cancellationToken);
                }

                public SpecifiedRecordsModifying ChangeToMatch<TRecordData>(
                    RecordsBatchTransactionSpecification
                        .ReadOnlyIncompatible
                        .SpecifiedRecord
                        .SpecificationToMatchAfterDataChanging<TRecord, TRecordData> specification)
                    where TRecordData : class
                {
                    if (specification == null) throw new ArgumentNullException(nameof(specification));
                    var transaction = Parent.Transaction ?? throw new ApplicationException("transaction == null");
                    if (!Parent.CheckIsSpecified(specification.Identity)) throw new ApplicationException("!wrapper.Parent.CheckIsSpecified(specification.Identity)");
                    transaction.For(specification);
                    return this;
                }
            }
        }
    }
}