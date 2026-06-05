using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition
{
    public static partial class ComposableRecordsBatchTransaction
    {
        public abstract partial class ConfiguredIdentically<TRecord, TRecordData>
            : ComposingRecords<TRecord, TRecordData>
            where TRecord : class
            where TRecordData : class
        {
            protected virtual RecordsBatchTransactionSpecification GetSupportedRecordDataSpecificationToAdd(
                RecordsBatchTransactionSpecification handlingSpecification)
            {
                CurrentRunState.EnsureIsConfiguration();

                if (handlingSpecification is AlwaysTrueSpecifications.IIgnoredByPersistence)
                    throw new ApplicationException("handlingSpecification is AlwaysTrueSpecifications.IIgnoredByPersistence");
                if (!(handlingSpecification is RecordsBatchTransactionSpecification.ISpecificationWrapper))
                    throw new ApplicationException("!(handlingSpecification is RecordsBatchTransactionSpecification.ISpecificationWrapper)");

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

            protected sealed override bool HandleReadOnlyRecordsIncludingBeforeRead()
            {
                CurrentRunState.EnsureIsAuthorized();
                UseCurrentRecordType();
                return HandleReadOnlyRecordsIncludingBeforeReadWithCurrentRecordType();
            }

            protected virtual bool HandleReadOnlyRecordsIncludingBeforeReadWithCurrentRecordType() => false;

            protected sealed override bool HandleEachRecordSpecificationBeforeReadWithoutIds(
                ISpecification<TRecordData> specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                if (ByIds) throw new ApplicationException("ByIds");
                CurrentRunState.EnsureIsAuthorized();
                UseCurrentRecordType();
                return HandleEachRecordSpecificationBeforeReadWithoutIdsWithCurrentRecordType(specification);
            }

            protected virtual bool HandleEachRecordSpecificationBeforeReadWithoutIdsWithCurrentRecordType(
                ISpecification<TRecordData> specification) => false;

            protected sealed override bool HandleEachRecordModifyingSpecificationBeforeRead(
                ISpecification<TRecordData> specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                CurrentRunState.EnsureIsAuthorized();
                UseCurrentRecordTypeAndIncludeForChanges();
                return HandleEachRecordModifyingSpecificationBeforeReadWithCurrentRecordType(specification);
            }

            protected virtual bool HandleEachRecordModifyingSpecificationBeforeReadWithCurrentRecordType(
                ISpecification<TRecordData> specification) => false;

            protected sealed override Task<int> ReadWithHandledEachRecordSpecifications(
                bool lockForChangesPersisting, int batchSize, int recordsCountToSkip, CancellationToken cancellationToken)
            {
                CurrentRunState.EnsureIsReadingStarted();
                ResetCurrentRecordType();
                return ParallelReadWithHandledEachRecordSpecifications(
                    lockForChangesPersisting, batchSize, recordsCountToSkip, cancellationToken);
            }

            protected virtual Task<int> ParallelReadWithHandledEachRecordSpecifications(
                bool lockForChangesPersisting, int batchSize, int recordsCountToSkip, CancellationToken cancellationToken)
                => throw new ApplicationException(nameof(ParallelReadWithHandledEachRecordSpecifications));

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

            protected sealed override bool HandleSpecifiedRecordModifyingSpecificationBeforePersist(
                int recordIndex, Identity identity, ISpecification<TRecordData> specification)
            {
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                CurrentRunState.EnsureIsChangesAssertionStarted();
                UseCurrentRecordTypeAndIncludeForChanges();
                return HandleSpecifiedRecordModifyingSpecificationBeforePersistWithCurrentRecordType(
                    recordIndex, identity, specification);
            }

            protected abstract bool HandleSpecifiedRecordModifyingSpecificationBeforePersistWithCurrentRecordType(
                int recordIndex, Identity identity, ISpecification<TRecordData> specification);

            protected sealed override Task PersistChanges(CancellationToken cancellationToken)
            {
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                CurrentRunState.EnsureIsPersistChangesStarted();
                ResetCurrentRecordType();
                return ParallelPersistChanges(cancellationToken);
            }

            protected abstract Task ParallelPersistChanges(CancellationToken cancellationToken);

            protected override void TriggerAfterRecordDataLocking(TransactionCallback.ForStateAssertion.TriggeredAfterRecordDataLocking eventArgs)
            {
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                CurrentRunState.EnsureIsPersistChangesFinished();
                UseCurrentRecordType();
                base.TriggerAfterRecordDataLocking(eventArgs);
            }

            protected sealed override Task FinishRecordsDataAccess(CancellationToken cancellationToken)
            {
                if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                CurrentRunState.EnsureIsDataAccessFinished();
                ResetCurrentRecordType();
                return ParallelFinishRecordsDataAccess(cancellationToken);
            }

            protected virtual Task ParallelFinishRecordsDataAccess(CancellationToken cancellationToken) => Task.CompletedTask;

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