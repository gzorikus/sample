using System;

namespace YourCompany.OLTP.RecordsManagement.UseCases.TransactionalComposition
{
    public static partial class ComposableRecordsBatchTransaction
    {
        public abstract partial class ConfiguredIdentically<TRecord, TRecordData>
        {
            public abstract class NonRepositoryRecords : ConfiguredIdentically<TRecord, TRecordData>,
                IConfiguredIdentically
            {
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