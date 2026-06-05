using System;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransactionConfiguration<TRecord>
    {
        public readonly partial struct NoSorting
        {
            public readonly struct Paginated
            {
                internal RecordsBatchTransaction.ISpecifiedRun<TRecord> Transaction { get; }

                internal Paginated(RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction)
                    => Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));

                public Task<IRecordsBatch<TRecord>> Run(CancellationToken cancellationToken)
                {
                    var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                    return transaction.Run(cancellationToken);
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
                    transaction.For(new RecordsBatchTransactionSpecification
                        .ReadOnlyIncompatible
                        .SpecifiedRecord
                        .UseCaseParameters(
                            identity, TransactionCallback.ForStateAssertion.TriggeredBeforeDataChanging.Default));
                    return new ReadOnly(transaction);
                }

                public EachRecordModifying TriggerUseCase(EventArgs useCaseParameters)
                {
                    var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                    transaction.For(new RecordsBatchTransactionSpecification
                        .ReadOnlyIncompatible
                        .UseCaseParameters(useCaseParameters));
                    return new EachRecordModifying(transaction);
                }

                public SpecifiedRecordsModifying TriggerUseCase(EventArgs useCaseParameters, Identity identity)
                {
                    var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                    transaction.For(new RecordsBatchTransactionSpecification
                        .ReadOnlyIncompatible
                        .SpecifiedRecord.UseCaseParameters(identity, useCaseParameters));
                    return new SpecifiedRecordsModifying(transaction);
                }

                public EachRecordModifying ChangeToMatch<TRecordData>(
                    RecordsBatchTransactionSpecification
                        .ReadOnlyIncompatible
                        .SpecificationToMatchAfterDataChanging<TRecord, TRecordData> specification)
                    where TRecordData : class
                {
                    if (specification == null) throw new ArgumentNullException(nameof(specification));
                    var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                    transaction.For(specification);
                    return new EachRecordModifying(transaction);
                }

                public SpecifiedRecordsModifying ChangeToMatch<TRecordData>(
                    RecordsBatchTransactionSpecification
                        .ReadOnlyIncompatible
                        .SpecifiedRecord
                        .SpecificationToMatchAfterDataChanging<TRecord, TRecordData> specification)
                    where TRecordData : class
                {
                    if (specification == null) throw new ArgumentNullException(nameof(specification));
                    var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                    transaction.For(specification);
                    return new SpecifiedRecordsModifying(transaction);
                }
            }
        }
    }
}