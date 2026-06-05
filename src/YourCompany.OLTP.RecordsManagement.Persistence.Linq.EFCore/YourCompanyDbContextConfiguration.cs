using System.Collections.Generic;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore
{
    public class YourCompanyDbContextConfiguration : Configuration.EFCore.YourCompanyDbContextConfiguration
    {
        public int DefaultMaxRecordsInBatch { get; init; } = 50;

        public IReadOnlyDictionary<string, YourCompanyDbContextConfigurationByType> ByRecordDataTypeFullName { get; init; }

        public readonly struct YourCompanyDbContextConfigurationByType
        {
            public int? MaxRecordsInBatch { get; init; }
        }
    }
}