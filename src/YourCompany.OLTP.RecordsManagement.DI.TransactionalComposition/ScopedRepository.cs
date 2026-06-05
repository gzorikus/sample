using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.RecordsManagement.Persistence;
using YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection;

namespace YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition
{
    internal class ScopedRepository<TRecord> : IRepository<TRecord> where TRecord : class
    {
        private readonly ScopedRecordsBatchTransactionFactory _recordBatchTransactionFactory;
        private readonly IComposableRecordsDataAccessProvider _dataAccessProvider;

        public ScopedRepository(
            ScopedRecordsBatchTransactionFactory recordBatchTransactionFactory,
            IComposableRecordsDataAccessProvider dataAccessProvider)
        {
            _recordBatchTransactionFactory = recordBatchTransactionFactory ?? throw new ArgumentNullException(nameof(recordBatchTransactionFactory));
            _dataAccessProvider = dataAccessProvider ?? throw new ArgumentNullException(nameof(dataAccessProvider));
        }

        public async Task<RecordsBatchTransaction.ISpecifiedRun<TRecord>> AccessRecordsBatch(
            CancellationToken cancellationToken)
        {
            var repositoryRecordTypeInfo = _recordBatchTransactionFactory.GetRecordTypeInfo(typeof(TRecord));

            ComposableRecordTypeInfo entityRecordTypeInfo;

            if (repositoryRecordTypeInfo.IsMixin)
            {
                entityRecordTypeInfo = _recordBatchTransactionFactory.GetMixinRequestingEntityRecordTypeInfo(
                    repositoryRecordTypeInfo);
            }
            else if (repositoryRecordTypeInfo.IsEntity)
            {
                entityRecordTypeInfo = repositoryRecordTypeInfo;
            }
            else
            {
                throw new ApplicationException("!repositoryRecordTypeInfo.IsMixin && !repositoryRecordTypeInfo.IsEntity");
            }

            var recordsDataAccess = await _dataAccessProvider.Connect(entityRecordTypeInfo.RecordDataType, cancellationToken);

            var orderedRecordTypes = entityRecordTypeInfo.ComposableTypeInfo.RootOrderedComposables
                ?? throw new ApplicationException("entityRecordTypeInfo.ComposableTypeInfo.RootOrderedComposables");
            var composedTransactions = new ComposableRecordsBatchTransaction.IComposingRecords[orderedRecordTypes.Count];
            var recordsDataAccessProxy = ProxyDataAccess(
                recordsDataAccess, repositoryRecordTypeInfo, entityRecordTypeInfo, composedTransactions);

            try
            {
                RecordsBatchTransaction.ISpecifiedRun<TRecord> repositoryTransaction = null;

                for (int i = 0; i < composedTransactions.Length; i++)
                {
                    var composableRecordTypeInfo = entityRecordTypeInfo.Map[orderedRecordTypes[i].Type];
                    var transaction = (ComposableRecordsBatchTransaction.IComposingRecords)
                        _recordBatchTransactionFactory.CreateTransaction(
                            composableRecordTypeInfo.RecordTypeInfo, recordsDataAccessProxy);

                    if (transaction is RecordsBatchTransaction.ISpecifiedRun<TRecord> requestedTransaction)
                    {
                        if (repositoryTransaction != null) throw new ApplicationException("repositoryTransaction != null");
                        repositoryTransaction = requestedTransaction;
                    }

                    transaction.RecordTypeInfo = composableRecordTypeInfo;
                    composedTransactions[i] = transaction;
                }

                return repositoryTransaction ?? throw new ApplicationException("repositoryTransaction == null");
            }
            catch
            {
                RecordsDataAccess.IAsyncDisposableWithCancellation disposable = recordsDataAccessProxy;
                await disposable.Dispose(cancellationToken);
                throw;
            }
        }

        protected virtual ComposingRecordsDataAccessProxy ProxyDataAccess(IComposableRecordsDataAccess recordsDataAccess,
            ComposableRecordTypeInfo repositoryRecordTypeInfo,
            ComposableRecordTypeInfo entityRecordTypeInfo,
            IReadOnlyList<ComposableRecordsBatchTransaction.IComposingRecords> composedTransactions)
            => new(recordsDataAccess, repositoryRecordTypeInfo, entityRecordTypeInfo, composedTransactions);
    }
}