using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.StateOwnership;
using YourCompany.Threading;

namespace YourCompany.OLTP.RecordsManagement.UseCases
{
    public abstract partial class RecordsBatchTransaction<TRecord, TRecordData>
        : RecordsBatchTransactionUseCases.IAuthorizerSpecificationsCollector<TRecordData>
    {
        private AwaitTasksList _awaitTasksList;

        protected virtual AwaitTasksList AwaitTasksList => _awaitTasksList = _awaitTasksList ?? new AwaitTasksList();

        protected override Task Authorize(CancellationToken cancellationToken)
        {
            CurrentRunState.EnsureIsStarted();
            return EnumerateRecordsBatchTransactionAuthorizersIfAny()?.AuthorizeToSpecifiedTransaction(
                TransactionSpecifications,
                authorizingSpecificationsCollector: this,
                AwaitTasksList,
                cancellationToken) ?? Task.CompletedTask;
        }

        protected virtual IEnumerable<RecordsBatchTransactionUseCases.IAuthorizer<TRecordData>>
            EnumerateRecordsBatchTransactionAuthorizersIfAny() => null;

        void RecordsBatchTransactionUseCases.IAuthorizerSpecificationsCollector<TRecordData>.Collect(
            ISpecification<TRecordData> authorizingSpecification)
        {
            CurrentRunState.EnsureIsStarted();
            if (_awaitTasksList == null) throw new ApplicationException("_awaitTasksList == null");
            lock (_awaitTasksList) Authorize(authorizingSpecification);
        }

        protected override async Task HandleResultingRecordsBatch(CancellationToken cancellationToken)
        {
            if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
            CurrentRunState.EnsureIsResultingRecordsBatchHandling();

            try
            {
                EnumerateRecordsBatchTransactionFinishingHandlersWithoutRecordsIfAny()?
                    .AddHandleRecordsBatchAfterDataAccessTransactionCallbacksTasks(this, AwaitTasksList, cancellationToken);
                EnumerateRecordsBatchTransactionFinishingHandlersWithRecordsIfAny()?
                    .AddHandleRecordsBatchAfterDataAccessTransactionCallbacksTasks(this, AwaitTasksList, cancellationToken);
            }
            finally
            {
                await WaitTasksOnce();
            }
        }

        protected virtual IEnumerable<RecordsBatchTransactionUseCases.IFinishingHandler>
            EnumerateRecordsBatchTransactionFinishingHandlersWithoutRecordsIfAny() => null;

        protected virtual IEnumerable<RecordsBatchTransactionUseCases.IFinishingHandler<TRecord>>
            EnumerateRecordsBatchTransactionFinishingHandlersWithRecordsIfAny() => null;
        
        protected Task WaitTasksOnce() => _awaitTasksList?.WaitOnce() ?? Task.CompletedTask;
    }
}