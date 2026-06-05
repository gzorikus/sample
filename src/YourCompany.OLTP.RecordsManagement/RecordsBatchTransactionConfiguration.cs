using System;
using System.Threading;
using System.Threading.Tasks;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransactionConfiguration<TRecord> where TRecord : class
    {
        public readonly struct ReadOnly
        {
            internal RecordsBatchTransaction.ISpecifiedRun<TRecord> Transaction { get; }

            internal ReadOnly(RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction)
                => Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));

            public Task<IRecordsBatch<TRecord>> Run(CancellationToken cancellationToken)
            {
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                return transaction.Run(cancellationToken);
            }
        }

        public readonly struct EachRecordModifying
        {
            internal RecordsBatchTransaction.ISpecifiedRun<TRecord> Transaction { get; }

            internal EachRecordModifying(RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction)
                => Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));

            public Task<IRecordsBatch<TRecord>> Run(CancellationToken cancellationToken)
            {
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                return transaction.Run(cancellationToken);
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
                return this;
            }
        }

        public readonly struct SpecifiedRecordsModifying
        {
            internal RecordsBatchTransaction.ISpecifiedRun<TRecord> Transaction { get; }

            internal SpecifiedRecordsModifying(RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction)
                => Transaction = transaction ?? throw new ArgumentNullException(nameof(transaction));

            public Task<IRecordsBatch<TRecord>> Run(CancellationToken cancellationToken)
            {
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
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
                var transaction = Transaction ?? throw new ApplicationException("transaction == null");
                transaction.For(specification);
                return this;
            }
        }
    }
}