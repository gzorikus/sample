using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.RecordsManagement.Persistence;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition
{
    internal static partial class ComposableRecordsBatchTransaction
    {
        internal abstract partial class ConfiguredIdentically<TRecord, TRecordData>
            : ComposingRecords<TRecord, TRecordData>
            where TRecord : class
            where TRecordData : class
        {
            private ConfiguredIdentically(
                ScopedRecordsBatchTransactionFactory provider, RecordsDataAccess.IStarting recordsDataAccess)
                : base(provider, recordsDataAccess) { }

            protected virtual RecordsBatchTransactionSpecification GetSupportedRecordDataSpecificationToAdd(
                RecordsBatchTransactionSpecification handlingSpecification)
            {
                CurrentRunState.EnsureIsConfiguration();

                if (handlingSpecification is AlwaysTrueSpecifications.IIgnoredByPersistence)
                    throw new ApplicationException("handlingSpecification is AlwaysTrueSpecifications.IIgnoredByPersistence");
                if (handlingSpecification is not RecordsBatchTransactionSpecification.ISpecificationWrapper)
                    throw new ApplicationException("handlingSpecification is not RecordsBatchTransactionSpecification.ISpecificationWrapper");

                if (handlingSpecification is ISpecification<TRecordData>) return handlingSpecification;

                if (handlingSpecification is RecordsBatchTransactionSpecification.ReadOnlyIncompatible)
                {
                    if (handlingSpecification is RecordsBatchTransactionSpecification
                        .ReadOnlyIncompatible
                        .SpecifiedRecord specifiedRecord)
                    {
                        if (!AnySpecificationsToMatchAfterDataChanging)
                            return new AlwaysTrueSpecifications.SpecifiedRecordAfterDataChanging<TRecord, TRecordData>(
                                specifiedRecord.Identity);
                    }
                    else
                    {
                        if (!AnySpecificationsToMatchAfterDataChangingEachRecord)
                            return AlwaysTrueSpecifications.EachRecordAfterDataChanging<TRecord, TRecordData>.Instance;
                    }
                }
                else
                {
                    if (!AnySpecificationsToMatchWithoutDataChangesEachRecord)
                        return AlwaysTrueSpecifications.EachRecordWithoutDataChanges<TRecord, TRecordData>.Instance;
                }

                return null;
            }

            protected override bool HandleReadOnlyRecordsIncludingBeforeRead()
            {
                CurrentRunState.EnsureIsAuthorized();
                UseCurrentRecordType();
                return base.HandleReadOnlyRecordsIncludingBeforeRead();
            }

            protected override bool HandleEachRecordSpecificationBeforeReadWithoutIds(
                ISpecification<TRecordData> specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                if (ByIds) throw new ApplicationException("ByIds");
                CurrentRunState.EnsureIsAuthorized();
                UseCurrentRecordType();
                return base.HandleEachRecordSpecificationBeforeReadWithoutIds(specification);
            }

            protected override bool HandleEachRecordModifyingSpecificationBeforeRead(
                ISpecification<TRecordData> specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                CurrentRunState.EnsureIsAuthorized();
                UseCurrentRecordTypeAndIncludeForChanges();
                return base.HandleEachRecordModifyingSpecificationBeforeRead(specification);
            }

            protected override Task<int> ReadWithHandledEachRecordSpecifications(
                bool lockForChangesPersisting, int batchSize, int recordsCountToSkip, CancellationToken cancellationToken)
            {
                CurrentRunState.EnsureIsReadingStarted();
                ResetCurrentRecordType();
                return base.ReadWithHandledEachRecordSpecifications(
                    lockForChangesPersisting, batchSize, recordsCountToSkip, cancellationToken);
            }

            protected override void HandleReadIdentities(int readRecordsCount)
            {
                CurrentRunState.EnsureIsReadingStarted();
                ResetCurrentRecordType();
                base.HandleReadIdentities(readRecordsCount);
            }

            protected override IReadOnlyList<TRecord> CreateRecords(IReadOnlyList<Identity> identities)
            {
                CurrentRunState.EnsureIsRecordsBuildingStarted();
                UseCurrentRecordType();
                return base.CreateRecords(identities);
            }

            protected override bool HandleSpecifiedRecordModifyingSpecificationBeforePersist(
                int recordIndex, Identity identity, ISpecification<TRecordData> specification)
            {
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                CurrentRunState.EnsureIsChangesAssertionStarted();
                UseCurrentRecordTypeAndIncludeForChanges();
                return base.HandleSpecifiedRecordModifyingSpecificationBeforePersist(recordIndex, identity, specification);
            }

            protected override Task PersistChanges(CancellationToken cancellationToken)
            {
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                CurrentRunState.EnsureIsPersistChangesStarted();
                ResetCurrentRecordType();
                return base.PersistChanges(cancellationToken);
            }

            protected override void TriggerAfterRecordDataLocking(
                TransactionCallback.ForStateAssertion.TriggeredAfterRecordDataLocking eventArgs)
            {
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                CurrentRunState.EnsureIsPersistChangesFinished();
                UseCurrentRecordType();
                base.TriggerAfterRecordDataLocking(eventArgs);
            }

            protected override Task FinishRecordsDataAccess(CancellationToken cancellationToken)
            {
                if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                CurrentRunState.EnsureIsDataAccessFinished();
                ResetCurrentRecordType();
                return base.FinishRecordsDataAccess(cancellationToken);
            }

            protected override void TriggerAfterDataAccess(
                TransactionCallback.ForStateAssertion.TriggeredAfterDataAccess eventArgs)
            {
                if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                CurrentRunState.EnsureIsDataAccessFinished();
                UseCurrentRecordType();
                base.TriggerAfterDataAccess(eventArgs);
            }
        }
    }
}