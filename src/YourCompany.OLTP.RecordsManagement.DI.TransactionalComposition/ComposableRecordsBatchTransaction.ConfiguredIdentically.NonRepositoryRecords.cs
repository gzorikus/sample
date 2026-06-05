using System;

namespace YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition
{
    internal static partial class ComposableRecordsBatchTransaction
    {
        internal abstract partial class ConfiguredIdentically<TRecord, TRecordData>
        {
            internal abstract class NonRepositoryRecords : ConfiguredIdentically<TRecord, TRecordData>,
                IConfiguredIdentically
            {
                internal NonRepositoryRecords(
                    DI.ScopedRecordsBatchTransactionFactory provider, ComposingRecordsDataAccessProxy recordsDataAccess)
                    : base(provider, recordsDataAccess) { }

                bool IConfiguredIdentically.Add(RecordsBatchTransactionSpecification specification)
                    => Add(specification);

                bool IConfiguredIdentically.AddSupportedRecordDataSpecification(
                    RecordsBatchTransactionSpecification specification)
                    => AddSupportedRecordDataSpecification(specification);

                void IConfiguredIdentically.FinishConfiguration(bool includeForChanges)
                {
                    FinishConfiguration();
                    if (includeForChanges) UseCurrentRecordTypeAndIncludeForChanges();
                }

                protected override void TriggerUseCaseParameters()
                    => throw new ApplicationException(nameof(NonRepositoryRecords));

                protected bool AddSupportedRecordDataSpecification(RecordsBatchTransactionSpecification specification)
                {
                    var specificationToAdd = GetSupportedRecordDataSpecificationToAdd(specification);
                    return specificationToAdd == null || Add(specificationToAdd);
                }
            }
        }
    }
}