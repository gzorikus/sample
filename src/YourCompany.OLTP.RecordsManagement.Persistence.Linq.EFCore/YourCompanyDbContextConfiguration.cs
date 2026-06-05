using System.Collections.Generic;
using YourCompany.Configuration.EFCore.PessimisticLocking;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore
{
    public class YourCompanyDbContextConfiguration : Configuration.EFCore.YourCompanyDbContextConfiguration
    {
        public bool EnableStateOwnershipConflictingChangesDetection { get; init; }
        public int DefaultMaxRecordsInBatch { get; init; } = 50;

        public string DefaultLastModifiedAtPropertyName { get; init; }
            = PessimisticLockingUpdateInterceptionContext.DefaultLastModifiedAtColumnName;

        public string DefaultLastModifiedAtColumnName { get; init; }
            = PessimisticLockingUpdateInterceptionContext.DefaultLastModifiedAtColumnName;

        public IReadOnlyDictionary<string, YourCompanyDbContextConfigurationByType> ByRecordDataTypeFullName { get; init; }

        public readonly struct YourCompanyDbContextConfigurationByType
        {
            public int? MaxRecordsInBatch { get; init; }
            public string LastModifiedAtPropertyName { get; init; }
            public string LastModifiedAtColumnName { get; init; }
        }
    }
}