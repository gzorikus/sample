using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.RecordsManagement.Persistence;
using YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection;

namespace YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition
{
    internal sealed class ComposingRecordsDataAccessProxy : RecordsDataAccess.Proxy,
        RecordsDataAccess.IReadOnly,
        RecordsDataAccess.IReadBeforeModifying,
        RecordsDataAccess.IModifying,
        RecordsDataAccess.IFinish
    {
        private readonly IComposableRecordsDataAccess _original;
        private int _readTimesRemaining;
        private TaskCompletionSource<int> _readCompletionSource;
        private int _persistTimesRemaining;
        private TaskCompletionSource<RecordsDataAccess.IFinish> _persistCompletionSource;
        private int _finishTimesRemaining;
        private TaskCompletionSource _finishCompletionSource;

        internal ComposableRecordTypeInfo RepositoryRecordTypeInfo { get; }
        internal ComposableRecordTypeInfo EntityRecordTypeInfo { get; }
        internal IReadOnlyList<ComposableRecordsBatchTransaction.IComposingRecords> ComposedTransactions { get; }

        internal ComposingRecordsDataAccessProxy(IComposableRecordsDataAccess original,
            ComposableRecordTypeInfo repositoryRecordTypeInfo,
            ComposableRecordTypeInfo entityRecordTypeInfo,
            IReadOnlyList<ComposableRecordsBatchTransaction.IComposingRecords> composedTransactions)
            : base(original)
        {
            if (original == null) throw new ArgumentNullException(nameof(original));
            if (repositoryRecordTypeInfo == null) throw new ArgumentNullException(nameof(repositoryRecordTypeInfo));
            if (entityRecordTypeInfo == null) throw new ArgumentNullException(nameof(entityRecordTypeInfo));
            if (composedTransactions == null) throw new ArgumentNullException(nameof(composedTransactions));

            if (!entityRecordTypeInfo.IsEntity) throw new ApplicationException("!entityRecordTypeInfo.IsEntity");
            if (entityRecordTypeInfo.ComposableTypeInfo.RootOrderedComposables == null)
                throw new ApplicationException("entityRecordTypeInfo.ComposableTypeInfo.RootOrderedComposables == null");
            if (entityRecordTypeInfo.ComposableTypeInfo.RootOrderedComposables.Count != composedTransactions.Count)
                throw new ApplicationException("entityRecordTypeInfo.ComposableTypeInfo.RootOrderedComposables.Count != composedTransactions.Count");
            if (!entityRecordTypeInfo.ComposableTypeInfo.CheckRootOrderedComposablesContain(repositoryRecordTypeInfo.Type))
                throw new ApplicationException("!entityRecordTypeInfo.ComposableTypeInfo.CheckRootOrderedComposablesContain(repositoryRecordTypeInfo.Type)");
            if (!repositoryRecordTypeInfo.IsMixin && repositoryRecordTypeInfo.Type != entityRecordTypeInfo.Type)
                throw new ApplicationException("!repositoryRecordTypeInfo.IsMixin && repositoryRecordTypeInfo.Type != entityRecordTypeInfo.Type");
            if (original.CurrentRecordDataType != null) throw new ApplicationException("original.CurrentRecordDataType != null");

            _original = original;
            RepositoryRecordTypeInfo = repositoryRecordTypeInfo;
            EntityRecordTypeInfo = entityRecordTypeInfo;
            ComposedTransactions = composedTransactions ?? throw new ArgumentNullException(nameof(composedTransactions));
        }

        internal void SetCurrentRecordTypeAndIncludeForChanges(ComposableRecordTypeInfo recordTypeInfo)
        {
            SetCurrentRecordType(recordTypeInfo);
            _original.EnsureRecordsDataLockingWithoutChanges(recordTypeInfo.RecordDataType);
        }

        internal void SetCurrentRecordType(ComposableRecordTypeInfo recordTypeInfo)
        {
            if (recordTypeInfo == null) throw new ArgumentNullException(nameof(recordTypeInfo));
            if (!EntityRecordTypeInfo.ComposableTypeInfo.CheckRootOrderedComposablesContain(recordTypeInfo.Type))
                throw new ApplicationException("!EntityRecordTypeInfo.ComposableTypeInfo.CheckRootOrderedComposablesContain(recordTypeInfo.Type)");
            _original.CurrentRecordDataType = recordTypeInfo.RecordDataType;
        }

        internal void ResetCurrentRecordType()
        {
            _original.CurrentRecordDataType = null;
        }

        internal void PrepareToRead()
        {
            if (_readCompletionSource != null) throw new ApplicationException("_readCompletionSource != null");
            _readTimesRemaining = ComposedTransactions.Count;
            _readCompletionSource = new TaskCompletionSource<int>();
        }

        internal void PrepareToPersist()
        {
            if (_persistCompletionSource != null) throw new ApplicationException("_persistCompletionSource != null");
            _persistTimesRemaining = ComposedTransactions.Count;
            _persistCompletionSource = new TaskCompletionSource<RecordsDataAccess.IFinish>();
        }

        internal void PrepareToFinish()
        {
            if (_finishCompletionSource != null) throw new ApplicationException("_finishCompletionSource != null");
            _finishTimesRemaining = ComposedTransactions.Count;
            _finishCompletionSource = new TaskCompletionSource();
        }

        internal void DecrementFinishingTransactions()
        {
            if (DecrementAndGetFinishTimesRemaining() == 0) throw new ApplicationException("DecrementAndGetFinishTimesRemaining() == 0");
        }

        internal void EnsureNoPendingTasks()
        {
            _readCompletionSource?.TrySetResult(0);
            _persistCompletionSource?.TrySetResult(null);
            _finishCompletionSource?.TrySetResult();
        }

        internal bool CheckToContinueIteratingAfterPersist(ComposableRecordTypeInfo recordTypeInfo)
        {
            if (recordTypeInfo == null) throw new ArgumentNullException(nameof(recordTypeInfo));
            if (!EntityRecordTypeInfo.ComposableTypeInfo.CheckRootOrderedComposablesContain(recordTypeInfo.Type))
                throw new ApplicationException("!EntityRecordTypeInfo.ComposableTypeInfo.CheckRootOrderedComposablesContain(recordTypeInfo.Type)");
            return _original.CheckHasLockedRecordsData(recordTypeInfo.RecordDataType);
        }

        Task<int> RecordsDataAccess.IReadOnly.ReadWithoutModifying(
            int batchSize, int recordsCountToSkip, CancellationToken cancellationToken)
            => DecrementAndGetReadTimesRemaining() == 0
                ? FinishReading(ReadWithoutModifying(batchSize, recordsCountToSkip, cancellationToken))
                : FinishReading(_readCompletionSource.Task);

        Task<int> RecordsDataAccess.IReadBeforeModifying.ReadAndLockForChangesPersisting(
            int batchSize, int recordsCountToSkip, CancellationToken cancellationToken)
            => DecrementAndGetReadTimesRemaining() == 0
                ? FinishReading(ReadAndLockForChangesPersisting(batchSize, recordsCountToSkip, cancellationToken))
                : FinishReading(_readCompletionSource.Task);

        Task RecordsDataAccess.IModifying.PersistChanges(CancellationToken cancellationToken)
            => DecrementAndGetPersistTimesRemaining() == 0
                ? FinishModifying(PersistChanges(cancellationToken))
                : FinishModifying(_persistCompletionSource.Task);

        Task RecordsDataAccess.IFinish.Finish(CancellationToken cancellationToken)
            => DecrementAndGetFinishTimesRemaining() == 0
                ? Finish(cancellationToken)
                : _finishCompletionSource.Task;

        private int DecrementAndGetReadTimesRemaining()
        {
            if (_readCompletionSource == null) throw new ApplicationException("_readCompletionSource == null");
            int remainingBefore = Interlocked.Decrement(ref _readTimesRemaining);
            if (remainingBefore >= ComposedTransactions.Count) throw new ApplicationException("remainingBefore >= ComposedTransactions.Count");
            if (remainingBefore < 1) throw new ApplicationException("remainingBefore < 1");
            return remainingBefore - 1;
        }

        private async Task<int> ReadWithoutModifying(
            int batchSize, int recordsCountToSkip, CancellationToken cancellationToken)
        {
            if (_readCompletionSource == null) throw new ApplicationException("_readCompletionSource == null");
            int readRecordsCount = await GetFinishingChain().ReadWithoutModifying(
                batchSize, recordsCountToSkip, cancellationToken);
            _readCompletionSource.SetResult(readRecordsCount);
            return readRecordsCount;
        }

        private async Task<int> ReadAndLockForChangesPersisting(
            int batchSize, int recordsCountToSkip, CancellationToken cancellationToken)
        {
            if (_readCompletionSource == null) throw new ApplicationException("_readCompletionSource == null");
            int readRecordsCount = await GetFinishingChain().ReadAndLockForChangesPersisting(
                batchSize, recordsCountToSkip, cancellationToken);
            _readCompletionSource.SetResult(readRecordsCount);
            return readRecordsCount;
        }

        private int DecrementAndGetPersistTimesRemaining()
        {
            if (_persistCompletionSource == null) throw new ApplicationException("_persistCompletionSource == null");
            int remainingBefore = Interlocked.Decrement(ref _persistTimesRemaining);
            if (remainingBefore >= ComposedTransactions.Count) throw new ApplicationException("remainingBefore >= ComposedTransactions.Count");
            if (remainingBefore < 1) throw new ApplicationException("remainingBefore < 1");
            return remainingBefore - 1;
        }

        private async Task<RecordsDataAccess.IFinish> PersistChanges(CancellationToken cancellationToken)
        {
            if (_persistCompletionSource == null) throw new ApplicationException("_persistCompletionSource == null");
            var finish = await GetFinishingChain().PersistChanges(cancellationToken);
            _persistCompletionSource.SetResult(finish);
            return finish;
        }

        private int DecrementAndGetFinishTimesRemaining()
        {
            if (_finishCompletionSource == null) throw new ApplicationException("_finishCompletionSource == null");
            int remainingBefore = Interlocked.Decrement(ref _finishTimesRemaining);
            if (remainingBefore >= ComposedTransactions.Count) throw new ApplicationException("remainingBefore >= ComposedTransactions.Count");
            if (remainingBefore < 1) throw new ApplicationException("remainingBefore < 1");
            return remainingBefore - 1;
        }

        private async Task Finish(CancellationToken cancellationToken)
        {
            if (_finishCompletionSource == null) throw new ApplicationException("_finishCompletionSource == null");
            await GetFinishingChain().Finish(cancellationToken);
            _finishCompletionSource.SetResult();
        }
    }
}