using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement.Persistence
{
    public abstract class RecordsBatchTransactionDataBridge<TRecord, TRecordData>
        : RecordsBatchTransaction.WithRecords<TRecord>.WithRecordsData<TRecordData>
        where TRecord : class
        where TRecordData : class
    {
        private RecordsDataAccess.IAsyncDisposableWithCancellation _recordsDataAccess;
        private RecordsDataAccess.State _recordsDataAccessState;

        public sealed override int MaxRecordsInBatch { get; }
        protected RecordsDataAccess.State CurrentRecordsDataAccessState => _recordsDataAccessState;

        protected RecordsBatchTransactionDataBridge(RecordsDataAccess.IStarting recordsDataAccess)
        {
            _recordsDataAccess = recordsDataAccess ?? throw new ArgumentNullException(nameof(recordsDataAccess));
            _recordsDataAccessState = new RecordsDataAccess.State(recordsDataAccess);
            MaxRecordsInBatch = recordsDataAccess.MaxRecordsInBatch;
        }

        protected override bool HandleSpecifiedIdentities(IReadOnlyList<Identity> orderedIdentities, bool skipMissing)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            if (skipMissing != ByIdsSkipMissing) throw new ApplicationException("skipMissing != ByIdsSkipMissing");
            CurrentRunState.EnsureIsConfiguration();
            _recordsDataAccessState = _recordsDataAccessState.HandleSpecifiedIdentities(orderedIdentities, skipMissing);
            return !_recordsDataAccessState.WasResetToDefault();
        }

        protected override bool HandleSortingAfterId(Identity lastSortedIdentity)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            CurrentRunState.EnsureIsConfiguration();
            _recordsDataAccessState = _recordsDataAccessState.HandleSortingAfterId(lastSortedIdentity);
            return !_recordsDataAccessState.WasResetToDefault();
        }

        protected override bool CheckToRead()
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            return base.CheckToRead()
                || _recordsDataAccessState.ForceReadByIdsForIdentityReplacementIfModifying;
        }

        protected override bool HandleReadOnlyRecordsIncludingBeforeRead()
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            CurrentRunState.EnsureIsAuthorized();
            _recordsDataAccessState = _recordsDataAccessState.HandleReadOnlyRecordsIncludingBeforeRead();
            return !_recordsDataAccessState.WasResetToDefault();
        }

        protected override bool HandleEachRecordSpecificationBeforeReadWithoutIds(ISpecification<TRecordData> specification)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (ByIds) throw new ApplicationException("ByIds");
            CurrentRunState.EnsureIsAuthorized();
            _recordsDataAccessState = _recordsDataAccessState.HandleEachRecordSpecificationBeforeReadWithoutIds(specification);
            return !_recordsDataAccessState.WasResetToDefault();
        }

        protected override bool HandleEachRecordModifyingSpecificationBeforeRead(ISpecification<TRecordData> specification)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (ReadOnly) throw new ApplicationException("ReadOnly");
            CurrentRunState.EnsureIsAuthorized();
            _recordsDataAccessState = _recordsDataAccessState.EnsureModifyingChainBeforeRead();
            return !_recordsDataAccessState.WasResetToDefault()
                && _recordsDataAccessState.GetFinishingChain().ChangeEachRecordToMatch(specification);
        }

        protected override async Task<int> ReadWithHandledEachRecordSpecifications(
            bool lockForChangesPersisting, int batchSize, int recordsCountToSkip, CancellationToken cancellationToken)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            CurrentRunState.EnsureIsReadingStarted();
            var readingState = lockForChangesPersisting
                ? _recordsDataAccessState.EnsureModifyingChainBeforeRead()
                : _recordsDataAccessState.EnsureReadOnlyChainBeforeRead();
            int readRecordsCount = lockForChangesPersisting 
                ? await readingState.GetFinishingChain().ReadAndLockForChangesPersisting(
                    batchSize, recordsCountToSkip, cancellationToken)
                : await readingState.GetFinishingChain().ReadWithoutModifying(
                    batchSize, recordsCountToSkip, cancellationToken);
            _recordsDataAccessState = readingState.FinishReading(readRecordsCount);
            return readRecordsCount;
        }

        protected override Identity GetReadRecordIdentity(int recordIndex, Identity originalIdentityWhenSpecified)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            CurrentRunState.EnsureIsReadingStarted();
            return _recordsDataAccessState.GetFinishingChain().GetReadRecordIdentity(
                recordIndex, originalIdentityWhenSpecified);
        }

        protected override TRecordData CreateRecordDataForSettingChangedProperties(
            int recordsCount, int recordIndex, Identity identity)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (ReadOnly) throw new ApplicationException("ReadOnly");
            CurrentRunState.EnsureIsRecordsBuildingStarted();
            _recordsDataAccessState = _recordsDataAccessState.EnsureModifyingChainBeforePersist();
            return (TRecordData)_recordsDataAccessState.GetFinishingChain().CreateRecordDataForSettingChangedProperties(
                recordIndex, identity);
        }

        protected override bool HandleSpecifiedRecordModifyingSpecificationBeforePersist(
            int recordIndex, Identity identity, ISpecification<TRecordData> specification)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (ReadOnly) throw new ApplicationException("ReadOnly");
            CurrentRunState.EnsureIsChangesAssertionStarted();
            _recordsDataAccessState = _recordsDataAccessState.EnsureModifyingChainBeforePersist();
            return _recordsDataAccessState.GetFinishingChain().ChangeSpecifiedRecordToMatch(
                recordIndex, identity, specification);
        }

        protected override async Task PersistChanges(CancellationToken cancellationToken)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            if (ReadOnly) throw new ApplicationException("ReadOnly");
            CurrentRunState.EnsureIsPersistChangesStarted();
            var finish = await _recordsDataAccessState.GetFinishingChain().PersistChanges(cancellationToken);
            _recordsDataAccessState = _recordsDataAccessState.FinishModifying(finish);
        }

        protected override bool ValidateAssignedIdentityReplacementAfterReadByIds(
            int recordIndex, Identity originalIdentity, Identity replacingIdentity)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            if (originalIdentity == null) throw new ArgumentNullException(nameof(originalIdentity));
            if (replacingIdentity == null) throw new ArgumentNullException(nameof(replacingIdentity));
            if (!ByIds) throw new ApplicationException("!ByIds");
            _recordsDataAccessState.GetFinishingChain().EnsureIsAterRead();
            if (!(replacingIdentity is UniqueKey uniqueKey)) throw new ApplicationException("!(replacingIdentity is UniqueKey)");
            var withoutExtraValues = uniqueKey.AsPrimaryKey()?.AsWithoutExtraValues() ?? throw new ApplicationException("uniqueKey.AsPrimaryKey()?.AsWithoutExtraValues()");
            return originalIdentity.CompareBy.Reference.Equals(withoutExtraValues.OriginalIdentityReference);
        }

        protected override TRecordData GetLockedRecordDataWithoutChanges(
            int recordIndex, Identity identity, BuiltRecord builtRecord)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (ReadOnly) throw new ApplicationException("ReadOnly");
            if (!builtRecord.CheckRecordWasBuilt()) throw new ApplicationException("!builtRecord.CheckRecordWasBuilt()");
            if (!builtRecord.BeforeDataChangingAssertionWasTriggered) throw new ApplicationException("!builtRecord.BeforeDataChangingAssertionWasTriggered");
            CurrentRunState.EnsureIsPersistChangesFinished();
            return (TRecordData)_recordsDataAccessState.GetFinishingChain().GetLockedRecordDataWithoutChanges(
                recordIndex, identity);
        }

        protected override Task FinishRecordsDataAccess(CancellationToken cancellationToken)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            CurrentRunState.EnsureIsDataAccessFinished();
            var finishing = ReadOnlyIncludeRecords
                ? _recordsDataAccessState.GetFinishingChain().WhenReadOnly().WhenAfterRead()
                : _recordsDataAccessState.GetFinishingChain().WhenModified();
            if (ReadOnlyIncludeRecords != finishing.ReadOnlyRecordsIncluded) throw new ApplicationException("ReadOnlyIncludeRecords != finishingChain.ReadOnlyRecordsIncluded");
            return finishing.Finish(cancellationToken);
        }

        protected override TRecordData GetRecordDataAfterAccess(int recordIndex, Identity identity, BuiltRecord builtRecord)
        {
            if (_recordsDataAccess == null) throw new ApplicationException("_recordsDataAccess == null");
            if (identity == null) throw new ArgumentNullException(nameof(identity));
            if (!builtRecord.CheckRecordWasBuilt()) throw new ApplicationException("!builtRecord.CheckRecordWasBuilt()");
            if (!builtRecord.BeforeDataChangingAssertionWasTriggered) throw new ApplicationException("!builtRecord.BeforeDataChangingAssertionWasTriggered");
            CurrentRunState.EnsureIsDataAccessFinished();
            var finishing = ReadOnlyIncludeRecords
                ? _recordsDataAccessState.GetFinishingChain().WhenReadOnly().WhenAfterRead()
                : _recordsDataAccessState.GetFinishingChain().WhenModified();
            if (ReadOnlyIncludeRecords != finishing.ReadOnlyRecordsIncluded) throw new ApplicationException("ReadOnlyIncludeRecords != finishingChain.ReadOnlyRecordsIncluded");
            return (TRecordData)finishing.GetRecordDataAfterAccess(recordIndex, identity);
        }

        protected override async Task Return(CancellationToken cancellationToken, Exception runException = null)
        {
            var recordsDataAccess = _recordsDataAccess ?? throw new ApplicationException("_recordsDataAccess == null");
            _recordsDataAccess = null;
            _recordsDataAccessState = default;

            Exception returnException = null;

            try
            {
                try
                {
                    await base.Return(cancellationToken, runException);
                }
                catch (Exception ex)
                {
                    returnException = ex;
                    throw;
                }
            }
            finally
            {
                try
                {
                    await recordsDataAccess.Dispose(cancellationToken);
                }
                catch (Exception) when (returnException == null)
                {
                    throw;
                }
                catch (Exception ex) when (returnException != null)
                {
                    throw new AggregateException(returnException, ex);
                }
            }
        }
    }
}