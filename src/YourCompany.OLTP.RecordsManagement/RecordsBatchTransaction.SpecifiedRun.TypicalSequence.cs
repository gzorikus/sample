using System;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class SpecifiedRun
        {
            protected virtual Task Authorize(CancellationToken cancellationToken) => Task.CompletedTask;
            protected virtual bool HandleReadOnlyRecordsIncludingBeforeRead() => false;
            protected virtual bool HandleEachRecordSpecificationsBeforeReadWithoutIds() => false;
            protected virtual bool HandleEachRecordModifyingSpecificationsBeforeRead() => false;
            protected virtual bool CheckToRead() => false;

            protected virtual Task<int> ReadWithHandledEachRecordSpecifications(
                bool lockForChangesPersisting, int batchSize, int recordsCountToSkip, CancellationToken cancellationToken)
                => throw new ApplicationException(nameof(ReadWithHandledEachRecordSpecifications));

            protected virtual void HandleReadIdentities(int readRecordsCount) { }

            protected abstract void BuildRecords();

            protected virtual void TriggerEachRecordSpecificationsToMatchWithoutDataChanges()
                => throw new ApplicationException(nameof(TriggerEachRecordSpecificationsToMatchWithoutDataChanges));

            protected virtual void PrepareRecordsBatchChanges() { }

            protected virtual void TriggerUseCaseParameters()
                => throw new ApplicationException(nameof(TriggerUseCaseParameters));

            protected virtual void TriggerSpecificationsToMatchAfterDataChanging()
                => throw new ApplicationException(nameof(TriggerSpecificationsToMatchAfterDataChanging));

            protected abstract void TriggerBeforeDataChanging(
                TransactionCallback.ForStateAssertion.TriggeredBeforeDataChanging eventArgs);

            protected abstract Task PersistChanges(CancellationToken cancellationToken);

            protected abstract void EnsureUniqueAssignedSpecifiedIdentitiesAfterPersist();

            protected abstract void TriggerAfterRecordDataLocking(
                TransactionCallback.ForStateAssertion.TriggeredAfterRecordDataLocking eventArgs);

            protected virtual Task FinishRecordsDataAccess(CancellationToken cancellationToken) => Task.CompletedTask;

            protected abstract void TriggerAfterDataAccess(
                TransactionCallback.ForStateAssertion.TriggeredAfterDataAccess eventArgs);

            protected virtual Task HandleResultingRecordsBatch(CancellationToken cancellationToken) => Task.CompletedTask;

            protected virtual Task Return(CancellationToken cancellationToken, Exception runException = null)
                => Task.CompletedTask;
        }
    }
}