using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class WithIdentities : SpecifiedRun, IRecordsBatch
        {
            private readonly List<Identity> _identities;
            private bool _identitiesSorting;

            public int PresentRecordsCount => ByIdsSkipMissing
                ? _identities.Count - ByIdsSkippedMissingRecords
                : _identities.Count;

            Type IRecordsBatch.RecordType => RecordType ?? throw new ApplicationException("RecordType == null");
            protected abstract Type RecordType { get; }
            protected override IRecordsBatch RecordsBatchAfterRun => this;
            public IReadOnlyList<Identity> Identities => _identities;
            protected WithIdentities() => _identities = new List<Identity>();

            protected override bool Handle(RecordsBatchTransactionSpecification.Sorting.ByIds specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                ByIdsSkipMissing = specification.SkipMissing;
                ReducedBatchSize = specification.OrderedIdentities.Count;

                if (_identities.Count > 0)
                {
                    if (_identitiesSorting)
                    {
                        if (_identities.Count != specification.OrderedIdentities.Count)
                            throw new ApplicationException("_identitiesSorting && _identities.Count != specification.OrderedIdentities.Count");

                        for (int recordIndex = 0; recordIndex < specification.OrderedIdentities.Count; recordIndex++)
                            if (!specification.OrderedIdentities[recordIndex].CompareBy.Reference.Equals(_identities[recordIndex]))
                                throw new ApplicationException("_identitiesSorting && !specification.OrderedIdentities[recordIndex].CompareBy.Reference.Equals(_identities[recordIndex])");
                    }
                    else
                    {
                        RestrictModifiedRecordOriginalIdentityReferencesToSortingIdentities(specification.OrderedIdentities);
                        _identities.Clear();
                        _identities.AddRange(specification.OrderedIdentities);
                    }
                }
                else
                {
                    _identities.AddRange(specification.OrderedIdentities);
                    _identitiesSorting = true;
                }

                return HandleSpecifiedIdentities(specification.OrderedIdentities, ByIdsSkipMissing);
            }

            protected virtual bool HandleSpecifiedIdentities(
                IReadOnlyList<Identity> orderedIdentities, bool skipMissing) => false;

            protected override bool Handle(RecordsBatchTransactionSpecification.Sorting.AfterId specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                if (_identities.Count > 0) throw new ApplicationException("_identities.Count > 0");
                if (_recordIndeciesByOriginalIdentityReference != null) throw new ApplicationException("_recordIndeciesByOriginalIdentityReference != null");
                AfterId = specification.LastSortedIdentity;
                return HandleSortingAfterId(specification.LastSortedIdentity);
            }

            protected virtual bool HandleSortingAfterId(Identity lastSortedIdentity) => false;

            protected override bool Handle(
                RecordsBatchTransactionSpecification.ReadOnlyIncompatible.SpecifiedRecord specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                ReadOnly = false;
                ByIds = true;

                if (TryAddModifiedRecordSpecifiedIdentityOriginalReference(specification.Identity))
                    _identities.Add(specification.Identity);

                return base.Handle(specification);
            }

            protected override bool Handle(RecordsBatchTransactionSpecification.SpecifiedRecordIncompatible specification)
            {
                if (_identities.Count > 0) throw new ApplicationException("_identities.Count > 0");
                if (_recordIndeciesByOriginalIdentityReference != null) throw new ApplicationException("_recordIndeciesByOriginalIdentityReference != null");
                return base.Handle(specification);
            }

            protected override void FinishConfiguration()
            {
                CurrentRunState.EnsureIsConfiguration();

                if (_identities.Count > 0 && !_identitiesSorting)
                {
                    ReadOnly = false;
                    ByIdsSkipMissing = false;
                    _identitiesSorting = true;
                    if (!HandleSpecifiedIdentities(_identities, skipMissing: false))
                        throw new ApplicationException("!HandleSpecifiedIdentities(_identities, skipMissing: false)");
                }

                if (ByIds)
                {
                    ReducedBatchSize = _identities.Count;
                    RecordsCountToSkip = 0;
                }

                base.FinishConfiguration();
            }

            protected override bool CheckToRead()
            {
                CurrentRunState.EnsureIsAfterConfiguration();
                return !ByIds || ReadOnly;
            }

            protected override void HandleReadIdentities(int readRecordsCount)
            {
                if (readRecordsCount < 0) throw new ArgumentOutOfRangeException(nameof(readRecordsCount), readRecordsCount, message: null);
                CurrentRunState.EnsureIsReadingStarted();

                if (_identities.Count == 0)
                {
                    if (ByIds) throw new ApplicationException("_identities.Count == 0 && ByIds");
                    if (AfterId != null && AfterId.CheckExpectsAssignment()) throw new ApplicationException("AfterId != null && AfterId.CheckExpectsAssignment()");
                    for (int recordIndex = 0; recordIndex < readRecordsCount; recordIndex++)
                    {
                        var identity = GetReadRecordIdentity(recordIndex, originalIdentityWhenSpecified: null)
                            ?? throw new ApplicationException("identity == null");
                        AddReadIdentity(recordIndex, identity);
                    }
                }
                else
                {
                    if (!ByIds) throw new ApplicationException("_identities.Count > 0 && !ByIds");
                    if (AfterId != null) throw new ApplicationException("_identities.Count > 0 && AfterId != null");
                    if (readRecordsCount > _identities.Count) throw new ApplicationException("readRecordsCount > _identities.Count");
                    if (readRecordsCount < _identities.Count && !ByIdsSkipMissing)
                        throw new ApplicationException("readRecordsCount < _identities.Count && !ByIdsSkipMissing");

                    for (int recordIndex = 0; recordIndex < _identities.Count; recordIndex++)
                    {
                        var originalIdentity = _identities[recordIndex] ?? throw new ApplicationException("originalIdentity == null");
                        var identity = GetReadRecordIdentity(recordIndex, originalIdentity);
                        ReplaceOriginalIdentityReferenceAfterRead(recordIndex, originalIdentity, identity);
                    }

                    if (readRecordsCount != PresentRecordsCount) throw new ApplicationException("readRecordsCount != PresentRecordsCount");
                    if (ReadOnly && readRecordsCount > 0) EnsureReadOnlyUniqueAssignedIdentitiesAfterReadByIds();
                }
            }

            protected abstract Identity GetReadRecordIdentity(int recordIndex, Identity originalIdentityWhenSpecified);

            protected sealed override void BuildRecords()
            {
                CurrentRunState.EnsureIsRecordsBuildingStarted();
                BuildRecords(_identities);
            }

            protected abstract void BuildRecords(IReadOnlyList<Identity> identities);

            protected override Task Return(CancellationToken cancellationToken, Exception runException = null)
            {
                _recordIndeciesByOriginalIdentityReference = null;
                return Task.CompletedTask;
            }
        }
    }
}