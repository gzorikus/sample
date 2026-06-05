using System;
using System.Collections.Generic;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        internal abstract partial class WithRecords<TRecord>
        {
            internal abstract partial class WithRecordsData<TRecordData> : RecordState<TRecordData>.IConfiguredRecordsBatch
            {
                public IReadOnlyList<ISpecification<TRecordData>> EachRecordSpecificationsToMatchWithoutDataChanges
                    => _eachRecordSpecificationsWithoutDataChanges;

                public IReadOnlyList<ISpecification<TRecordData>> EachRecordSpecificationsToMatchAfterDataChanging
                    => _eachRecordSpecificationsAfterDataChanging;

                protected void PreparePresentRecordDataChangingBeforeTriggering(
                    int recordIndex, Identity identity, BuiltRecord builtRecord)
                {
                    if (identity == null) throw new ArgumentNullException(nameof(identity));
                    if (!builtRecord.CheckRecordWasBuilt()) throw new ApplicationException("!builtRecord.CheckRecordWasBuilt()");
                    if (ReadOnly) throw new ApplicationException("ReadOnly");
                    if (!this.CheckRecordIsPresent(recordIndex)) throw new ApplicationException("!this.CheckRecordIsPresent(recordIndex)");
                    CurrentRunState.EnsureIsRecordsBuildingStarted();
                    builtRecord.State.SetBeforeDataChanging(
                        CreateRecordDataForSettingChangedProperties(Identities.Count, recordIndex, identity));
                }

                protected abstract TRecordData CreateRecordDataForSettingChangedProperties(
                    int recordsCount, int recordIndex, Identity identity);

                protected override void TriggerBeforeDataChanging(
                    TransactionCallback.ForStateAssertion.TriggeredBeforeDataChanging eventArgs)
                {
                    if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
                    if (ReadOnly) throw new ApplicationException("ReadOnly");
                    CurrentRunState.EnsureIsChangesAssertionStarted();

                    for (int recordIndex = 0; recordIndex < BuiltRecords.Count; recordIndex++)
                    {
                        if (!this.CheckRecordIsPresent(recordIndex)) continue;
                        var builtRecord = GetBuiltRecord(recordIndex);
                        builtRecord.State.EnsureIsBeforeDataChanging();

                        if (!builtRecord.BeforeDataChangingAssertionWasTriggered)
                        {
                            builtRecord = builtRecord.WithBeforeDataChangingAssertionTriggered();
                            _builtRecords[recordIndex] = builtRecord;
                            TriggerTransactionCallback(recordIndex, eventArgs);
                        }
                    }
                }

                void RecordState<TRecordData>.IConfiguredRecordsBatch.MatchBeforeDataChanging(
                    int recordIndex, ISpecification<TRecordData> specification)
                {
                    if (specification == null) throw new ArgumentNullException(nameof(specification));
                    if (specification is RecordsBatchTransactionSpecification
                        .ReadOnlyIncompatible
                        .ISpecificationWrapper)
                        throw new ApplicationException("specification is RecordsBatchTransactionSpecification.ReadOnlyIncompatible.ISpecificationWrapper");

                    CurrentRunState.EnsureIsChangesAssertionStarted();
                    if (specification.Unwrap() is EventArgs eventArgs)
                        TriggerTransactionCallback(recordIndex, eventArgs);
                }

                void RecordState<TRecordData>.IConfiguredRecordsBatch.ChangeDataToMatch(
                    int recordIndex, ISpecification<TRecordData> specification)
                {
                    if (specification == null) throw new ArgumentNullException(nameof(specification));
                    if (ReadOnly) throw new ApplicationException("ReadOnly");
                    CurrentRunState.EnsureIsChangesAssertionStarted();
                    var identity = Identities[recordIndex] ?? throw new ApplicationException("identity == null");
                    var unwrapped = specification.Unwrap(identity);
                    TriggerTransactionCallbackBeforeDataChanging(recordIndex, unwrapped);
                    if (!HandleSpecifiedRecordModifyingSpecificationBeforePersist(recordIndex, identity, unwrapped))
                        throw new ApplicationException("!HandleSpecifiedRecordModifyingSpecificationBeforePersist(recordIndex, identity, unwrapped)");
                }

                protected abstract bool HandleSpecifiedRecordModifyingSpecificationBeforePersist(
                    int recordIndex, Identity identity, ISpecification<TRecordData> specification);

                protected override void TriggerAfterRecordDataLocking(
                    TransactionCallback.ForStateAssertion.TriggeredAfterRecordDataLocking eventArgs)
                {
                    if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
                    if (ReadOnly) throw new ApplicationException("ReadOnly");
                    CurrentRunState.EnsureIsPersistChangesFinished();

                    for (int recordIndex = 0; recordIndex < BuiltRecords.Count; recordIndex++)
                    {
                        if (!this.TryGetPresentRecordIdentity(recordIndex, out var identity)) continue;
                        var builtRecord = GetBuiltRecord(recordIndex);
                        if (!builtRecord.BeforeDataChangingAssertionWasTriggered) throw new ApplicationException("!builtRecord.BeforeDataChangingAssertionWasTriggered");
                        if (!builtRecord.State.CheckRecordDataIsLocked())
                            builtRecord.State.SetRecordDataIsLocked(
                                GetLockedRecordDataWithoutChanges(recordIndex, identity, builtRecord));
                        builtRecord.State.MatchingWithoutDataChanges.EnsureMatches(
                            identity, builtRecord.State.LockedRecordData);
                        TriggerTransactionCallback(recordIndex, eventArgs);
                    }
                }

                protected abstract TRecordData GetLockedRecordDataWithoutChanges(
                    int recordIndex, Identity identity, BuiltRecord builtRecord);

                protected override void TriggerAfterDataAccess(
                    TransactionCallback.ForStateAssertion.TriggeredAfterDataAccess eventArgs)
                {
                    if (eventArgs == null) throw new ArgumentNullException(nameof(eventArgs));
                    if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                    CurrentRunState.EnsureIsDataAccessFinished();

                    for (int recordIndex = 0; recordIndex < BuiltRecords.Count; recordIndex++)
                    {
                        if (!this.TryGetPresentRecordIdentity(recordIndex, out var identity)) continue;
                        var builtRecord = GetBuiltRecord(recordIndex);
                        if (!builtRecord.BeforeDataChangingAssertionWasTriggered) throw new ApplicationException("!builtRecord.BeforeDataChangingAssertionWasTriggered");
                        if (!builtRecord.State.CheckDataAccessIsFinished())
                            builtRecord.State.SetDataAccessIsFinished(
                                GetRecordDataAfterAccess(recordIndex, identity, builtRecord));
                        if (!ReadOnly)
                            builtRecord.State.MatchingAfterDataChanging.EnsureMatches(
                                identity, builtRecord.State.DataAfterAccess);
                        TriggerTransactionCallback(recordIndex, eventArgs);
                    }
                }

                protected abstract TRecordData GetRecordDataAfterAccess(
                    int recordIndex, Identity identity, BuiltRecord builtRecord);
            }
        }
    }
}