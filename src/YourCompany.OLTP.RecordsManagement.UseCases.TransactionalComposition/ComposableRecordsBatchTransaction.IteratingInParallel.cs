using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection;
using YourCompany.Threading;

namespace YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition
{
    public static partial class ComposableRecordsBatchTransaction
    {
        public abstract class IteratingInParallel<TRecord, TRecordData>
            : ConfiguredIdentically<TRecord, TRecordData>.RepositoryRecords,
            IIteratedInParallel
            where TRecord : class
            where TRecordData : class
        {
            private AwaitTasksList _runOncePendingTasks;

            IEnumerator<RunOnceStep> IIteratedInParallel.IterateRunOnceSteps(CancellationToken cancellationToken)
                => throw new ApplicationException(nameof(IteratingInParallel<TRecord, TRecordData>));

            protected override IEnumerable<RunOnceStep> IterateRunOnceSteps(CancellationToken cancellationToken)
            {
                var stepEnumerators = GetRunOnceStepsStartedTransactionEnumerators(cancellationToken);

                int firstNonInterruptedStepsEnumeratorIndex = 0;
                var firstNonInterruptedStepsEnumerator = stepEnumerators[0] ?? throw new ApplicationException("firstNonInterruptedStepsEnumerator == null");
                while (firstNonInterruptedStepsEnumerator.MoveNext())
                {
                    if (_runOncePendingTasks != null && _runOncePendingTasks.Count > 0)
                        yield return ThrowRunOncePendingTasksAware(new ApplicationException("_runOncePendingTasks != null && _runOncePendingTasks.Count > 0"));

                    CollectRunOncePendingTask(firstNonInterruptedStepsEnumerator.Current);

                    for (int i = firstNonInterruptedStepsEnumeratorIndex + 1; i < stepEnumerators.Count; i++)
                    {
                        var restNonInterruptedStepsEnumerator = stepEnumerators[i];
                        if (restNonInterruptedStepsEnumerator == null) continue;
                        if (!restNonInterruptedStepsEnumerator.MoveNext())
                            yield return ThrowRunOncePendingTasksAware(new ApplicationException("!restNonInterruptedStepsEnumerator.MoveNext()"));

                        CollectRunOncePendingTask(restNonInterruptedStepsEnumerator.Current);
                    }

                    yield return WaitAndClearRunOncePendingTasksStep();

                    if (CurrentRunState == State.PersistChangesFinished)
                    {
                        InterruptNotIncludedNonRepositoryTransactionStepsEnumeration(stepEnumerators);
                        firstNonInterruptedStepsEnumerator = stepEnumerators[firstNonInterruptedStepsEnumeratorIndex];

                        while (firstNonInterruptedStepsEnumerator == null)
                        {
                            if (++firstNonInterruptedStepsEnumeratorIndex >= stepEnumerators.Count)
                                throw new ApplicationException("++firstNonInterruptedStepsEnumeratorIndex >= stepEnumerators.Count");
                            firstNonInterruptedStepsEnumerator = stepEnumerators[firstNonInterruptedStepsEnumeratorIndex];
                        }
                    }
                }

                if (MoveNextAnyNonInterruptedEnumeratorAfterIteratingFinished(stepEnumerators))
                    yield return ThrowRunOncePendingTasksAware(new ApplicationException("MoveNextAnyNonInterruptedEnumeratorAfterIteratingFinished(stepEnumerators)"));
            }

            protected List<IEnumerator<RunOnceStep>> GetRunOnceStepsStartedTransactionEnumerators(
                CancellationToken cancellationToken)
            {
                if (_runOncePendingTasks != null) throw new ApplicationException("_runOncePendingTasks != null");
                CurrentRunState.EnsureIsStarted();

                bool thisFound = false;
                var composedTransactions = GetIteratedInParallelTransactions() ?? throw new ApplicationException("GetIteratedInParallelTransactions() == null");
                var stepEnumerators = new List<IEnumerator<RunOnceStep>>(composedTransactions.Count);

                for (int i = 0; i < composedTransactions.Count; i++)
                {
                    var composedTransaction = composedTransactions[i] ?? throw new ApplicationException("composedTransaction == null");
                    if (composedTransaction == this)
                    {
                        if (thisFound) throw new ApplicationException("thisFound");
                        thisFound = true;
                        stepEnumerators.Add(base.IterateRunOnceSteps(cancellationToken).GetEnumerator());
                    }
                    else if (!ReadOnly || CheckToIncludeNonRepositoryRecords(composedTransaction.RecordTypeInfo))
                    {
                        var stepsEnumerator = composedTransaction.IterateRunOnceSteps(cancellationToken)
                            ?? throw new ApplicationException("stepsEnumerator == null");
                        stepEnumerators.Add(stepsEnumerator);
                    }
                }

                if (!thisFound) throw new ApplicationException("!thisFound");
                if (stepEnumerators.Count < 1) throw new ApplicationException("stepEnumerators.Count < 1");
                return stepEnumerators;
            }

            protected virtual void InterruptNotIncludedNonRepositoryTransactionStepsEnumeration(
                List<IEnumerator<RunOnceStep>> stepEnumerators)
            {
                if (stepEnumerators == null) throw new ArgumentNullException(nameof(stepEnumerators));
                if (ReadOnly || ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly || ReadOnlyIncludeRecords");
                CurrentRunState.EnsureIsPersistChangesFinished();

                var composedTransactions = GetIteratedInParallelTransactions() ?? throw new ApplicationException("GetIteratedInParallelTransactions() == null");
                if (stepEnumerators.Count != composedTransactions.Count)
                    throw new ApplicationException("stepEnumerators.Count != composedTransactions.Count");

                for (int i = 0; i < composedTransactions.Count; i++)
                {
                    if (stepEnumerators[i] == null) throw new ApplicationException("stepEnumerators[i] == null");
                    var composedTransaction = composedTransactions[i] ?? throw new ApplicationException("transaction == null");

                    bool includeNonRepository = CheckToIncludeNonRepositoryRecords(composedTransaction.RecordTypeInfo);
                    bool continueAfterPersist = CheckToContinueIteratingAfterPersist(composedTransaction.RecordTypeInfo);

                    if (composedTransaction == this && !continueAfterPersist) throw new ApplicationException("includeNonRepository && !continueAfterPersist");
                    if (includeNonRepository && !continueAfterPersist) throw new ApplicationException("includeNonRepository && !continueAfterPersist");
                    if (composedTransaction == this || includeNonRepository || continueAfterPersist)
                        continue;

                    stepEnumerators[i] = null;
                }
            }

            protected abstract bool CheckToContinueIteratingAfterPersist(ComposableRecordTypeInfo recordTypeInfo);

            protected override async Task Return(CancellationToken cancellationToken, Exception runException = null)
            {
                var runOncePendingTasks = _runOncePendingTasks;
                _runOncePendingTasks = null;

                try
                {
                    await base.Return(cancellationToken, runException);
                }
                finally
                {
                    if (runOncePendingTasks != null) await runOncePendingTasks.WaitOnce();
                }
            }

            protected abstract IReadOnlyList<IIteratedInParallel> GetIteratedInParallelTransactions();

            private bool MoveNextAnyNonInterruptedEnumeratorAfterIteratingFinished(
                IReadOnlyList<IEnumerator<RunOnceStep>> stepEnumerators)
            {
                if (stepEnumerators == null) throw new ArgumentNullException(nameof(stepEnumerators));
                for (int i = 0; i < stepEnumerators.Count; i++)
                {
                    var nonInterruptedStepsEnumerator = stepEnumerators[i];
                    if (nonInterruptedStepsEnumerator == null) continue;
                    if (nonInterruptedStepsEnumerator.MoveNext())
                    {
                        CollectRunOncePendingTask(nonInterruptedStepsEnumerator.Current);
                        return true;
                    }
                }

                return false;
            }

            private void CollectRunOncePendingTask(RunOnceStep step)
            {
                if (step.AsyncStepTask != null)
                {
                    _runOncePendingTasks = _runOncePendingTasks ?? new AwaitTasksList();
                    _runOncePendingTasks.Add(step.AsyncStepTask);
                }

                if (step.State != CurrentRunState) ThrowRunOncePendingTasksAware(new ApplicationException("step.State != CurrentRunState"));
            }

            private RunOnceStep WaitAndClearRunOncePendingTasksStep() => new RunOnceStep(
                CurrentRunState,
                _runOncePendingTasks?.Count > 0
                    ? _runOncePendingTasks.WaitOnce()
                    : null);

            private RunOnceStep ThrowRunOncePendingTasksAware(Exception exception)
            {
                if (_runOncePendingTasks == null) throw exception;
                return new RunOnceStep(CurrentRunState, _runOncePendingTasks.ThrowAndClear(exception));
            }
        }
    }
}