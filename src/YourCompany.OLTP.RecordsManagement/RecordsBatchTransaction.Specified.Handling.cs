using System;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class Specified
        {
            protected virtual bool Handle(RecordsBatchTransactionSpecification specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                EnsureIsConfigurable();

                if (specification is RecordsBatchTransactionSpecification.Sorting)
                {
                    if (specification is RecordsBatchTransactionSpecification.Sorting.ByIds sortingByIds)
                    {
                        ByIdsSkipMissing = sortingByIds.SkipMissing;
                        return Handle(sortingByIds);
                    }

                    if (specification is RecordsBatchTransactionSpecification.Sorting.AfterId sortingAfterId)
                    {
                        AfterId = sortingAfterId.LastSortedIdentity;
                        return Handle(sortingAfterId);
                    }

                    return false;
                }

                if (specification is RecordsBatchTransactionSpecification.ReadOnly readOnly)
                {
                    ReadOnly = true;
                    return Handle(readOnly);
                }

                if (specification is RecordsBatchTransactionSpecification.ISpecificationWrapper matchWithoutDataChanges
                    && !(specification is RecordsBatchTransactionSpecification.ReadOnlyIncompatible))
                {
                    AnySpecificationsToMatchWithoutDataChangesEachRecord = true;
                    return Handle(matchWithoutDataChanges);
                }

                if (specification is RecordsBatchTransactionSpecification.ReadOnlyIncompatible readOnlyIncompatible)
                {
                    ReadOnly = false;
                    return Handle(readOnlyIncompatible);
                }

                if (specification is RecordsBatchTransactionSpecification.SpecifiedRecordIncompatible specifiedRecordsIncompatible)
                {
                    ByIds = false;
                    return Handle(specifiedRecordsIncompatible);
                }

                return false;
            }

            protected virtual bool Handle(RecordsBatchTransactionSpecification.Sorting.ByIds specification) => false;
            protected virtual bool Handle(RecordsBatchTransactionSpecification.Sorting.AfterId specification) => false;

            protected virtual bool Handle(RecordsBatchTransactionSpecification.ReadOnly specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                EnsureIsConfigurable();

                ReadOnly = true;

                if (specification is RecordsBatchTransactionSpecification.ReadOnly.IncludeRecords)
                {
                    ReadOnlyIncludeRecords = true;
                    return true;
                }

                return false;
            }

            protected virtual bool Handle(RecordsBatchTransactionSpecification.ISpecificationWrapper specification)
                => false;

            protected virtual bool Handle(RecordsBatchTransactionSpecification.ReadOnlyIncompatible specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                EnsureIsConfigurable();

                ReadOnly = false;

                if (specification is RecordsBatchTransactionSpecification
                    .ReadOnlyIncompatible
                    .UseCaseParameters useCaseParameters)
                {
                    AnyUseCaseParametersEachRecord = true;
                    return Handle(useCaseParameters);
                }

                if (specification is RecordsBatchTransactionSpecification
                    .ReadOnlyIncompatible
                    .ISpecificationWrapper matchAfterDataChanging
                    && !(specification is RecordsBatchTransactionSpecification.ReadOnlyIncompatible.SpecifiedRecord))
                {
                    AnySpecificationsToMatchAfterDataChangingEachRecord = true;
                    return Handle(matchAfterDataChanging);
                }

                if (specification is RecordsBatchTransactionSpecification
                    .ReadOnlyIncompatible
                    .SpecifiedRecord specifiedRecord)
                {
                    ByIds = true;
                    return Handle(specifiedRecord);
                }

                return false;
            }

            protected virtual bool Handle(
                RecordsBatchTransactionSpecification.ReadOnlyIncompatible.UseCaseParameters specification) => false;

            protected virtual bool Handle(
                RecordsBatchTransactionSpecification.ReadOnlyIncompatible.ISpecificationWrapper specification) => false;

            protected virtual bool Handle(
                RecordsBatchTransactionSpecification.ReadOnlyIncompatible.SpecifiedRecord specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                EnsureIsConfigurable();

                ReadOnly = false;
                ByIds = true;

                if (specification is RecordsBatchTransactionSpecification
                    .ReadOnlyIncompatible
                    .SpecifiedRecord.UseCaseParameters useCaseParameters)
                {
                    AnyUseCaseParameters = true;
                    return Handle(useCaseParameters);
                }
                else if (specification is RecordsBatchTransactionSpecification
                    .ReadOnlyIncompatible
                    .ISpecificationWrapper matchAfterDataChanging)
                {
                    AnySpecificationsToMatchAfterDataChanging = true;
                    return Handle(matchAfterDataChanging);
                }

                return false;
            }

            protected virtual bool Handle(
                RecordsBatchTransactionSpecification.ReadOnlyIncompatible.SpecifiedRecord.UseCaseParameters specification)
                => false;

            protected virtual bool Handle(RecordsBatchTransactionSpecification.SpecifiedRecordIncompatible specification)
            {
                if (specification == null) throw new ArgumentNullException(nameof(specification));
                EnsureIsConfigurable();

                ByIds = false;

                if (specification is RecordsBatchTransactionSpecification.SpecifiedRecordIncompatible.Paginate paginate)
                {
                    if (_recordsCountToSkip.HasValue) throw new ApplicationException("_recordsCountToSkip.HasValue");
                    ReducedBatchSize = paginate.ReducedBatchSize ?? MaxRecordsInBatch;
                    RecordsCountToSkip = paginate.RecordsCountToSkip;
                    return true;
                }

                return false;
            }
        }
    }
}