using System;
using YourCompany.OLTP.StateOwnership.Reflection;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore
{
    public class YourCompanyDbContextConfiguratorsLoadingContext
        : Configuration.EFCore.YourCompanyDbContextConfiguratorsLoadingContext
    {
        private RecordTypesMap _recordTypesMap;

        public RecordTypesMap RecordTypesMap
        {
            get => _recordTypesMap ?? throw new ApplicationException("_recordTypesMap == null");
            internal set
            {
                if (_recordTypesMap != null) throw new ApplicationException("_recordTypesMap != null");
                _recordTypesMap = value ?? throw new ApplicationException("value == null");
            }
        }
    }
}