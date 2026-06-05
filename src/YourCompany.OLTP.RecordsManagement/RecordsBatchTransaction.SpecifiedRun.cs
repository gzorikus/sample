using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class SpecifiedRun : Specified, ISpecifiedRun
        {
            public State CurrentRunState { get; private set; }
            protected abstract IRecordsBatch RecordsBatchAfterRun { get; }
            protected SpecifiedRun() => CurrentRunState = State.Configuration;

            async Task<IRecordsBatch> ISpecifiedRun.Run(CancellationToken cancellationToken)
            {
                await RunOnce(cancellationToken);
                CurrentRunState.EnsureIsReturning();
                return RecordsBatchAfterRun ?? throw new ApplicationException("RecordsBatchAfterRun == null");
            }

            protected sealed override void EnsureIsConfigurable() => CurrentRunState.EnsureIsConfiguration();

            protected virtual async Task RunOnce(CancellationToken cancellationToken)
            {
                CurrentRunState.EnsureIsConfiguration();

                FinishConfiguration();

                Exception runException = null;

                try
                {
                    try
                    {
                        foreach (var step in IterateRunOnceSteps(cancellationToken))
                            if (step.AsyncStepTask != null)
                                await step.AsyncStepTask;
                    }
                    catch (Exception ex)
                    {
                        runException = ex;
                        throw;
                    }
                }
                finally
                {
                    CurrentRunState = State.Returning;
                    await Return(cancellationToken, runException);
                }
            }

            protected override void FinishConfiguration()
            {
                base.FinishConfiguration();
                CurrentRunState = State.Started;
            }

            protected virtual IEnumerable<RunOnceStep> IterateRunOnceSteps(CancellationToken cancellationToken)
            {
                CurrentRunState.EnsureIsStarted();

                var authorizingTask = Authorize(cancellationToken) ?? throw new ApplicationException("authorizingTask == null");
                yield return new RunOnceStep(CurrentRunState, authorizingTask);
                if (!authorizingTask.IsCompleted) throw new ApplicationException("!authorizingTask.IsCompleted");

                yield return new RunOnceStep(state: CurrentRunState = State.Authorized);

                bool breakAfterNoRecordsRead = false;

                if (CheckToRead())
                {
                    if (ReadOnlyIncludeRecords && !HandleReadOnlyRecordsIncludingBeforeRead())
                        throw new ApplicationException("!HandleReadOnlyRecordsIncludingBeforeRead()");

                    if (AnySpecificationsToMatchWithoutDataChangesEachRecord && !ByIds && !HandleEachRecordSpecificationsBeforeReadWithoutIds())
                        throw new ApplicationException("!ByIds && !HandleEachRecordSpecificationsBeforeReadWithoutIds()");

                    if (AnySpecificationsToMatchAfterDataChangingEachRecord && !HandleEachRecordModifyingSpecificationsBeforeRead())
                        throw new ApplicationException("!HandleEachRecordModifyingSpecificationsBeforeRead()");

                    Task<int> readingTask;

                    yield return new RunOnceStep(
                        state: CurrentRunState = State.ReadingStarted,
                        asyncStepTask: readingTask = ReadWithHandledEachRecordSpecifications(
                            lockForChangesPersisting: !ReadOnly, ReducedBatchSize, RecordsCountToSkip, cancellationToken)
                                ?? throw new ApplicationException("readingTask == null"));
                    if (!readingTask.IsCompleted) throw new ApplicationException("!readingTask.IsCompleted");

                    int readRecordsCount = readingTask.Result;
                    if (readRecordsCount < 0) throw new ApplicationException("readRecordsCount < 0");
                    if (readRecordsCount > ReducedBatchSize) throw new ApplicationException("readRecordsCount > ReduceBatchSize");
                    HandleReadIdentities(readRecordsCount);
                    breakAfterNoRecordsRead = readRecordsCount == 0;
                }

                if (!ReadOnly || ReadOnlyIncludeRecords)
                {
                    yield return new RunOnceStep(state: CurrentRunState = State.RecordsBuildingStarted);
                    BuildRecords();
                    if (AnySpecificationsToMatchWithoutDataChangesEachRecord) TriggerEachRecordSpecificationsToMatchWithoutDataChanges();
                }

                if (breakAfterNoRecordsRead) yield break;

                if (!ReadOnly)
                {
                    PrepareRecordsBatchChanges();

                    yield return new RunOnceStep(state: CurrentRunState = State.ChangesAssertionStarted);
                    if (AnyUseCaseParameters) TriggerUseCaseParameters();
                    if (AnySpecificationsToMatchAfterDataChanging) TriggerSpecificationsToMatchAfterDataChanging();
                    TriggerBeforeDataChanging(TransactionCallback.ForStateAssertion.TriggeredBeforeDataChanging.Default);

                    Task persistingTask;
                    yield return new RunOnceStep(
                        state: CurrentRunState = State.PersistChangesStarted,
                        asyncStepTask: persistingTask = PersistChanges(cancellationToken)
                            ?? throw new ApplicationException("persistingTask == null"));
                    if (!persistingTask.IsCompleted) throw new ApplicationException("!persistingTask.IsCompleted");
                    yield return new RunOnceStep(state: CurrentRunState = State.PersistChangesFinished);

                    if (ByIds) EnsureUniqueAssignedSpecifiedIdentitiesAfterPersist();
                    TriggerAfterRecordDataLocking(
                        TransactionCallback.ForStateAssertion.TriggeredAfterRecordDataLocking.Default);
                }

                CurrentRunState = State.DataAccessFinished;

                if (!ReadOnly || ReadOnlyIncludeRecords)
                {
                    Task dataAccessFinishingTask;
                    yield return new RunOnceStep(
                        state: CurrentRunState,
                        asyncStepTask: dataAccessFinishingTask = FinishRecordsDataAccess(cancellationToken)
                            ?? throw new ApplicationException("dataAccessFinishingTask == null"));
                    if (!dataAccessFinishingTask.IsCompleted) throw new ApplicationException("!dataAccessFinishingTask.IsCompleted");

                    TriggerAfterDataAccess(TransactionCallback.ForStateAssertion.TriggeredAfterDataAccess.Default);
                }

                Task resultsHandlingTask;
                yield return new RunOnceStep(
                    state: CurrentRunState = State.ResultingRecordsBatchHandling,
                    asyncStepTask: resultsHandlingTask = HandleResultingRecordsBatch(cancellationToken)
                        ?? throw new ApplicationException("resultsHandlingTask == null"));
                if (!resultsHandlingTask.IsCompleted) throw new ApplicationException("!resultsHandlingTask.IsCompleted");
            }

            protected new void IncrementByIdsSkippedMissingRecords()
            {
                CurrentRunState.EnsureIsReadingStarted();
                base.IncrementByIdsSkippedMissingRecords();
            }
        }
    }
}