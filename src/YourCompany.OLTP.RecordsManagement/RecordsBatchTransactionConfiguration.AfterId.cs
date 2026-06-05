using System;
using System.Threading;
using System.Threading.Tasks;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransactionConfiguration<TRecord>
    {
        public readonly partial struct AfterId
        {
            internal RecordsBatchTransaction.ISpecifiedRun<TRecord> Transaction { get; }

            internal AfterId(RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction)
                => Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));

            public Task<IRecordsBatch<TRecord>> Run(CancellationToken cancellationToken)
            {
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                return transaction.Run(cancellationToken);
            }

            public AfterId WithFilter<TRecordData>(
                RecordsBatchTransactionSpecification
                    .SpecificationToMatchWithoutDataChanges<TRecord, TRecordData> specification)
                where TRecordData : class
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                transaction.For(specification);
                return this;
            }

            public Paginated TakeFirst() => Paginate(reducedBatchSize: 1);

            public Paginated Paginate(int? reducedBatchSize = null, int recordsCountToSkip = 0)
            {
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                transaction.Paginate(reducedBatchSize, recordsCountToSkip);
                return new Paginated(transaction);
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

            public EachRecordModifying TriggerUseCase(EventArgs useCaseParameters)
            {
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                transaction.For(new RecordsBatchTransactionSpecification
                    .ReadOnlyIncompatible
                    .UseCaseParameters(useCaseParameters));
                return new EachRecordModifying(transaction);
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
        }
    }
}