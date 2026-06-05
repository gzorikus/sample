using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class WithRecords<TRecord>
        {
            public abstract partial class WithRecordsData<TRecordData> : WithRecords<TRecord>,
                IReadOnlyList<TRecord>
                where TRecordData : class
            {
                private List<ISpecification<TRecordData>> _eachRecordSpecificationsWithoutDataChanges;
                private List<ISpecification<TRecordData>> _eachRecordSpecificationsAfterDataChanging;
                private BuiltRecord[] _builtRecords;

                TRecord IReadOnlyList<TRecord>.this[int index] => BuiltRecords[index].Record;
                int IReadOnlyCollection<TRecord>.Count => BuiltRecords.Count;
                IEnumerator<TRecord> IEnumerable<TRecord>.GetEnumerator() => EnumerateRecords();
                IEnumerator IEnumerable.GetEnumerator() => EnumerateRecords();

                protected IReadOnlyList<BuiltRecord> BuiltRecords => _builtRecords ?? throw new ApplicationException("_builtRecords == null");

                protected override bool Handle(RecordsBatchTransactionSpecification.ISpecificationWrapper specification)
                {
                    if (specification == null) throw new ArgumentNullException(nameof(specification));
                    CurrentRunState.EnsureIsConfiguration();
                    var handledEachRecordSpecification = specification as ISpecification<TRecordData>;
                    if (handledEachRecordSpecification == null) return false;
                    _eachRecordSpecificationsWithoutDataChanges = _eachRecordSpecificationsWithoutDataChanges
                        ?? new List<ISpecification<TRecordData>>();
                    _eachRecordSpecificationsWithoutDataChanges.Add(handledEachRecordSpecification);
                    return true;
                }

                protected override bool Handle(
                    RecordsBatchTransactionSpecification.ReadOnlyIncompatible.ISpecificationWrapper specification)
                {
                    if (specification == null) throw new ArgumentNullException(nameof(specification));
                    CurrentRunState.EnsureIsConfiguration();
                    var handledEachRecordSpecification = specification as ISpecification<TRecordData>;
                    if (handledEachRecordSpecification == null) return false;
                    if (specification.SpecifiedRecordIdentity != null) return true;
                    _eachRecordSpecificationsAfterDataChanging = _eachRecordSpecificationsAfterDataChanging
                        ?? new List<ISpecification<TRecordData>>();
                    _eachRecordSpecificationsAfterDataChanging.Add(handledEachRecordSpecification);
                    return true;
                }

                protected void Authorize(ISpecification<TRecordData> authorizingSpecification)
                {
                    if (authorizingSpecification == null) throw new ArgumentNullException(nameof(authorizingSpecification));
                    CurrentRunState.EnsureIsStarted();

                    _eachRecordSpecificationsWithoutDataChanges = _eachRecordSpecificationsWithoutDataChanges
                        ?? new List<ISpecification<TRecordData>>();

                    var unwrapped = authorizingSpecification.Unwrap();
                    for (int i = 0; i < _eachRecordSpecificationsWithoutDataChanges.Count; i++)
                        if (unwrapped.Equals(_eachRecordSpecificationsWithoutDataChanges[i].Unwrap()))
                            throw new ApplicationException("unwrapped.Equals(_eachRecordSpecificationsWithoutDataChanges[i].Unwrap())");

                    _eachRecordSpecificationsWithoutDataChanges.Add(authorizingSpecification);
                }

                protected override bool HandleEachRecordSpecificationsBeforeReadWithoutIds()
                {
                    if (!CheckToRead()) throw new ApplicationException("!CheckToRead()");
                    if (ByIds) throw new ApplicationException("ByIds");
                    CurrentRunState.EnsureIsAuthorized();

                    if (_eachRecordSpecificationsWithoutDataChanges != null)
                        for (int i = 0; i < _eachRecordSpecificationsWithoutDataChanges.Count; i++)
                            if (!HandleEachRecordSpecificationBeforeReadWithoutIds(
                                _eachRecordSpecificationsWithoutDataChanges[i].Unwrap()))
                                return false;

                    return true;
                }

                protected virtual bool HandleEachRecordSpecificationBeforeReadWithoutIds(
                    ISpecification<TRecordData> specification) => false;

                protected override bool HandleEachRecordModifyingSpecificationsBeforeRead()
                {
                    if (!CheckToRead()) throw new ApplicationException("!CheckToRead()");
                    CurrentRunState.EnsureIsAuthorized();

                    if (_eachRecordSpecificationsAfterDataChanging != null)
                        for (int i = 0; i < _eachRecordSpecificationsAfterDataChanging.Count; i++)
                            if (!HandleEachRecordModifyingSpecificationBeforeRead(
                                _eachRecordSpecificationsAfterDataChanging[i].Unwrap()))
                                return false;

                    return true;
                }

                protected virtual bool HandleEachRecordModifyingSpecificationBeforeRead(
                    ISpecification<TRecordData> specification) => false;

                protected override IReadOnlyList<TRecord> CreateRecords(IReadOnlyList<Identity> identities)
                {
                    if (identities == null) throw new ArgumentNullException(nameof(identities));
                    if (!ReferenceEquals(identities, Identities)) throw new ApplicationException("!ReferenceEquals(identities, Identities)");
                    if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                    CurrentRunState.EnsureIsRecordsBuildingStarted();

                    if (_builtRecords != null) throw new ApplicationException("_builtRecords != null");
                    _builtRecords = new BuiltRecord[identities.Count];

                    for (int recordIndex = 0; recordIndex < identities.Count; recordIndex++)
                    {
                        if (!this.TryGetPresentRecordIdentity(recordIndex, out var identity)) continue;
                        var builtRecord = BuildRecord(identities.Count, recordIndex, identity);
                        if (!builtRecord.CheckRecordWasBuilt()) throw new ApplicationException("!builtRecord.CheckRecordWasBuilt()");

                        if (!ReadOnly && !builtRecord.State.CheckIsBeforeDataChanging())
                            PreparePresentRecordDataChangingBeforeTriggering(recordIndex, identity, builtRecord);

                        _builtRecords[recordIndex] = builtRecord;
                    }

                    return this;
                }

                protected abstract BuiltRecord BuildRecord(int recordsCount, int recordIndex, Identity identity);

                protected override void TriggerEachRecordSpecificationsToMatchWithoutDataChanges()
                {
                    if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                    if (!AnySpecificationsToMatchWithoutDataChangesEachRecord) throw new ApplicationException("!AnySpecificationsToMatchWithoutDataChangesEachRecord");
                    if (_eachRecordSpecificationsWithoutDataChanges == null) throw new ApplicationException("_eachRecordSpecificationsWithoutDataChanges == null");
                    CurrentRunState.EnsureIsRecordsBuildingStarted();
                    for (int i = 0; i < _eachRecordSpecificationsWithoutDataChanges.Count; i++)
                        if (_eachRecordSpecificationsWithoutDataChanges[i].Unwrap() is EventArgs eventArgs)
                            for (int recordIndex = 0; recordIndex < BuiltRecords.Count; recordIndex++)
                                if (this.CheckRecordIsPresent(recordIndex))
                                    TriggerTransactionCallback(recordIndex, eventArgs);
                }

                protected override void TriggerUseCaseParameters()
                {
                    if (ReadOnly) throw new ApplicationException("ReadOnly");
                    if (!AnyUseCaseParameters) throw new ApplicationException("!AnyUseCaseParameters");
                    CurrentRunState.EnsureIsChangesAssertionStarted();

                    for (int i = 0; i < TransactionSpecifications.Count; i++)
                    {
                        var specification = TransactionSpecifications[i];
                        if (specification is RecordsBatchTransactionSpecification.ReadOnlyIncompatible)
                        {
                            if (specification is RecordsBatchTransactionSpecification
                                .ReadOnlyIncompatible.UseCaseParameters eachRecordUseCaseParameters)
                            {
                                for (int recordIndex = 0; recordIndex < BuiltRecords.Count; recordIndex++)
                                    if (this.CheckRecordIsPresent(recordIndex))
                                        TriggerTransactionCallbackBeforeDataChanging(
                                            recordIndex, eachRecordUseCaseParameters.UseCaseParametersToTrigger);
                            }
                            else if (specification is RecordsBatchTransactionSpecification
                                .ReadOnlyIncompatible
                                .SpecifiedRecord
                                .UseCaseParameters specifiedRecordUseCaseParameters)
                            {
                                if (TryGetRecordIndexByOriginalIdentityReference(
                                    specifiedRecordUseCaseParameters.Identity, out int recordIndex))
                                    TriggerTransactionCallbackBeforeDataChanging(
                                        recordIndex, specifiedRecordUseCaseParameters.UseCaseParametersToTrigger);
                            }
                        }
                    }
                }

                protected override void TriggerSpecificationsToMatchAfterDataChanging()
                {
                    if (ReadOnly) throw new ApplicationException("ReadOnly");
                    if (!AnySpecificationsToMatchAfterDataChanging) throw new ApplicationException("!AnySpecificationsToMatchAfterDataChanging");
                    CurrentRunState.EnsureIsChangesAssertionStarted();

                    for (int i = 0; i < TransactionSpecifications.Count; i++)
                    {
                        var specification = TransactionSpecifications[i];
                        if (specification is RecordsBatchTransactionSpecification.ReadOnlyIncompatible)
                        {
                            if (specification is RecordsBatchTransactionSpecification
                                .ReadOnlyIncompatible
                                .SpecificationToMatchAfterDataChanging<TRecord, TRecordData> eachRecordSpecification)
                            {
                                if (eachRecordSpecification.SpecificationToMatch is EventArgs eventArgs)
                                    for (int recordIndex = 0; recordIndex < BuiltRecords.Count; recordIndex++)
                                        if (this.CheckRecordIsPresent(recordIndex))
                                            TriggerTransactionCallbackBeforeDataChanging(recordIndex, eventArgs);
                            }
                            else if (specification is RecordsBatchTransactionSpecification
                                .ReadOnlyIncompatible
                                .SpecifiedRecord
                                .SpecificationToMatchAfterDataChanging<TRecord, TRecordData> specifiedRecordSpecification)
                            {
                                if (TryGetRecordIndexByOriginalIdentityReference(
                                    specifiedRecordSpecification.Identity, out int recordIndex))
                                    GetBuiltRecord(recordIndex).State.ChangeDataToMatch(specifiedRecordSpecification);
                            }
                        }
                    }
                }

                protected override Task Return(CancellationToken cancellationToken, Exception runException = null)
                {
                    _extraInterfaceProviders = null;
                    return base.Return(cancellationToken, runException);
                }

                protected BuiltRecord GetBuiltRecord(int recordIndex)
                {
                    var builtRecord = BuiltRecords[recordIndex];
                    if (!builtRecord.CheckRecordWasBuilt()) throw new ApplicationException("!builtRecord.CheckRecordWasBuilt()");
                    return builtRecord;
                }

                private IEnumerator<TRecord> EnumerateRecords()
                {
                    for (int recordIndex = 0; recordIndex < BuiltRecords.Count; recordIndex++)
                        yield return GetBuiltRecord(recordIndex).Record;
                }
            }
        }
    }
}