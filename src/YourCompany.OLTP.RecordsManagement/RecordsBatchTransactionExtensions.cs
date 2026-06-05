using System;
using System.Collections.Generic;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static class RecordsBatchTransactionExtensions
    {
        public static RecordsBatchTransactionConfiguration<TRecord>.ByIds WithId<TRecord>(
            this RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction, Identity identity, bool skipMissing = false)
            where TRecord : class
            => transaction.WithIds(identity).OrderingRecords(skipMissing);

        public static RecordsBatchTransactionConfiguration<TRecord>.ById WithIds<TRecord>(
            this RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction, Identity identity)
            where TRecord : class
        {
            var byId = new RecordsBatchTransactionConfiguration<TRecord>.ById(transaction);
            byId.Then(identity);
            return byId;
        }

        public static RecordsBatchTransactionConfiguration<TRecord>.ByIds WithIds<TRecord>(
            this RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction,
            IReadOnlyList<Identity> orderedIdentities,
            bool skipMissing = false)
            where TRecord : class
        {
            if (orderedIdentities == null) throw new ArgumentNullException(nameof(orderedIdentities));

            var byId = new RecordsBatchTransactionConfiguration<TRecord>.ById(transaction);

            for (int recordIndex = 0; recordIndex < orderedIdentities.Count; recordIndex++)
                byId.Then(orderedIdentities[recordIndex]);

            return byId.OrderingRecords(skipMissing);
        }

        public static RecordsBatchTransactionConfiguration<TRecord>.AfterId ContinueAfterSortedId<TRecord>(
            this RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction, Identity lastSortedIdentity)
            where TRecord : class
        {
            var afterId = new RecordsBatchTransactionConfiguration<TRecord>.AfterId(transaction);
            transaction.For(new RecordsBatchTransactionSpecification.Sorting.AfterId(lastSortedIdentity));
            return afterId;
        }

        public static RecordsBatchTransactionConfiguration<TRecord>.NoSorting WithoutSorting<TRecord>(
            this RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction)
            where TRecord : class
            => new RecordsBatchTransactionConfiguration<TRecord>.NoSorting(transaction);

        internal static void Paginate<TRecord>(
            this RecordsBatchTransaction.ISpecifiedRun<TRecord> transaction, int? reducedBatchSize, int recordsCountToSkip)
            where TRecord : class
        {
            if (reducedBatchSize.HasValue)
            {
                if (recordsCountToSkip > 0)
                {
                    transaction.For(new RecordsBatchTransactionSpecification
                        .SpecifiedRecordIncompatible
                        .Paginate
                        .Full(reducedBatchSize.Value, recordsCountToSkip));
                }
                else if (reducedBatchSize.Value == 1)
                {
                    transaction.For(RecordsBatchTransactionSpecification
                        .SpecifiedRecordIncompatible
                        .Paginate
                        .ReduceBatchSize.Single);
                }
                else
                {
                    transaction.For(new RecordsBatchTransactionSpecification
                        .SpecifiedRecordIncompatible
                        .Paginate
                        .ReduceBatchSize(reducedBatchSize.Value));
                }
            }
            else if (recordsCountToSkip > 0)
            {
                transaction.For(new RecordsBatchTransactionSpecification
                    .SpecifiedRecordIncompatible
                    .Paginate
                    .SkipRecords(recordsCountToSkip));
            }
        }
    }
}