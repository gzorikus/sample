using System;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class Specified
        {
            private int? _reducedBatchSize;
            private int? _recordsCountToSkip;
            private bool? _byIds;
            private int? _skippedMissingRecords;
            private Identity _afterId;
            private bool? _readOnly;
            private bool? _readOnlyIncludeRecords;
            private bool? _anySpecificationsToMatchWithoutDataChangesEachRecord;
            private bool? _anyUseCaseParameters;
            private bool? _anyUseCaseParametersEachRecord;
            private bool? _anySpecificationsToMatchAfterDataChanging;
            private bool? _anySpecificationsToMatchAfterDataChangingEachRecord;

            public int ReducedBatchSize
            {
                get => _reducedBatchSize ?? throw new ApplicationException("!_reducedBatchSize.HasValue");
                protected set
                {
                    if (value < 1 || value > MaxRecordsInBatch) throw new ArgumentOutOfRangeException(nameof(ReducedBatchSize), value, message: null);
                    if (_reducedBatchSize.HasValue && _reducedBatchSize.Value != value)
                        throw new ApplicationException("ReducedBatchSize != value");
                    EnsureIsConfigurable();
                    _reducedBatchSize = value;
                }
            }

            public int RecordsCountToSkip
            {
                get => _recordsCountToSkip ?? throw new ApplicationException("!_recordsCountToSkip.HasValue");
                protected set
                {
                    if (value < 0) throw new ArgumentOutOfRangeException(nameof(RecordsCountToSkip), value, message: null);
                    if (value > 0 && ByIds) throw new ApplicationException("RecordsCountToSkip > 0 && ByIds");
                    if (_recordsCountToSkip.HasValue && _recordsCountToSkip.Value != value)
                        throw new ApplicationException("RecordsCountToSkip != value");
                    EnsureIsConfigurable();
                    _recordsCountToSkip = value;
                }
            }

            public bool ByIds
            {
                get => _byIds ?? false;
                protected set
                {
                    if (value && AfterId != null) throw new ApplicationException("ByIds && AfterId != null");
                    if (_byIds.HasValue && _byIds.Value != value) throw new ApplicationException("ByIds != value");
                    EnsureIsConfigurable();
                    _byIds = value;
                    if (value) RecordsCountToSkip = 0;
                }
            }

            public bool ByIdsSkipMissing
            {
                get => _skippedMissingRecords.HasValue && _skippedMissingRecords >= 0;
                protected set
                {
                    if (_skippedMissingRecords.HasValue && _skippedMissingRecords > 0)
                        throw new ApplicationException("_skippedMissingRecords.HasValue && _skippedMissingRecords > 0");
                    if (_skippedMissingRecords.HasValue && ByIdsSkipMissing != value)
                        throw new ApplicationException("ByIdsSkipMissing != value");
                    ByIds = true;
                    _skippedMissingRecords = value ? 0 : -1;
                }
            }

            public Identity AfterId
            {
                get => _afterId;
                protected set
                {
                    if (value == null) throw new ArgumentNullException(nameof(AfterId));
                    if (_afterId != null && !_afterId.CompareBy.Reference.Equals(value))
                        throw new ApplicationException("!AfterId.CompareBy.Reference.Equals(value)");
                    ByIds = false;
                    _afterId = value;
                }
            }

            public bool ReadOnly
            {
                get => _readOnly ?? false;
                protected set
                {
                    if (_readOnly.HasValue && _readOnly.Value != value)
                        throw new ApplicationException("ReadOnly != value");
                    EnsureIsConfigurable();
                    _readOnly = value;
                }
            }

            public bool ReadOnlyIncludeRecords
            {
                get => _readOnlyIncludeRecords ?? false;
                protected set
                {
                    if (_readOnlyIncludeRecords.HasValue && _readOnlyIncludeRecords.Value != value)
                        throw new ApplicationException("ReadOnlyIncludeRecords != value");
                    ReadOnly = true;
                    _readOnlyIncludeRecords = value;
                }
            }

            public bool AnySpecificationsToMatchWithoutDataChangesEachRecord
            {
                get => _anySpecificationsToMatchWithoutDataChangesEachRecord ?? false;
                protected set
                {
                    if (_anySpecificationsToMatchWithoutDataChangesEachRecord.HasValue
                        && _anySpecificationsToMatchWithoutDataChangesEachRecord.Value != value)
                        throw new ApplicationException("AnySpecificationsToMatchWithoutDataChangesEachRecord != value");
                    EnsureIsConfigurable();
                    _anySpecificationsToMatchWithoutDataChangesEachRecord = value;
                }
            }

            public bool AnyUseCaseParameters
            {
                get => _anyUseCaseParameters ?? false;
                protected set
                {
                    if (_anyUseCaseParameters.HasValue && _anyUseCaseParameters.Value != value)
                        throw new ApplicationException("AnyUseCaseParameters != value)");
                    EnsureIsConfigurable();
                    _anyUseCaseParameters = value;
                    if (value) ReadOnly = false;
                }
            }

            public bool AnyUseCaseParametersEachRecord
            {
                get => _anyUseCaseParametersEachRecord ?? false;
                protected set
                {
                    if (_anyUseCaseParametersEachRecord.HasValue && _anyUseCaseParametersEachRecord.Value != value)
                        throw new ApplicationException("AnyUseCaseParametersEachRecord != value)");
                    AnyUseCaseParameters = true;
                    _anyUseCaseParametersEachRecord = value;
                }
            }

            public bool AnySpecificationsToMatchAfterDataChanging
            {
                get => _anySpecificationsToMatchAfterDataChanging ?? false;
                protected set
                {
                    if (_anySpecificationsToMatchAfterDataChanging.HasValue
                        && _anySpecificationsToMatchAfterDataChanging.Value != value)
                        throw new ApplicationException("AnySpecificationsToMatchAfterDataChanging != value");
                    EnsureIsConfigurable();
                    _anySpecificationsToMatchAfterDataChanging = value;
                    if (value) ReadOnly = false;
                }
            }

            public bool AnySpecificationsToMatchAfterDataChangingEachRecord
            {
                get => _anySpecificationsToMatchAfterDataChangingEachRecord ?? false;
                protected set
                {
                    if (_anySpecificationsToMatchAfterDataChangingEachRecord.HasValue
                        && _anySpecificationsToMatchAfterDataChangingEachRecord.Value != value)
                        throw new ApplicationException("AnySpecificationsToMatchAfterDataChangingEachRecord != value");
                    AnySpecificationsToMatchAfterDataChanging = true;
                    _anySpecificationsToMatchAfterDataChangingEachRecord = value;
                }
            }

            protected virtual void FinishConfiguration()
            {
                EnsureIsConfigurable();

                if (ByIds)
                {
                    if (!_reducedBatchSize.HasValue) throw new ApplicationException("ByIds && !_reducedBatchSize.HasValue");
                    if (!_skippedMissingRecords.HasValue) ByIdsSkipMissing = false;
                }
                else
                {
                    ByIds = false;
                    if (!_reducedBatchSize.HasValue) ReducedBatchSize = MaxRecordsInBatch;
                }

                if (!_recordsCountToSkip.HasValue) RecordsCountToSkip = 0;
                if (!_readOnly.HasValue) ReadOnly = true;
                if (ReadOnly && !_readOnlyIncludeRecords.HasValue) ReadOnlyIncludeRecords = false;
                if (!_anySpecificationsToMatchWithoutDataChangesEachRecord.HasValue) AnySpecificationsToMatchWithoutDataChangesEachRecord = false;
                if (!_anyUseCaseParameters.HasValue) AnyUseCaseParameters = false;
                if (!_anyUseCaseParametersEachRecord.HasValue) AnyUseCaseParametersEachRecord = false;
                if (!_anySpecificationsToMatchAfterDataChanging.HasValue) AnySpecificationsToMatchAfterDataChanging = false;
                if (!_anySpecificationsToMatchAfterDataChangingEachRecord.HasValue) AnySpecificationsToMatchAfterDataChangingEachRecord = false;
            }

            protected void IncrementByIdsSkippedMissingRecords()
            {
                if (!ByIds) throw new ApplicationException("!ByIds");
                if (ByIdsSkippedMissingRecords >= ReducedBatchSize) throw new ApplicationException("ByIdsSkippedMissingRecords >= ReducedBatchSize");
                _skippedMissingRecords = ByIdsSkippedMissingRecords + 1;
            }
        }
    }
}