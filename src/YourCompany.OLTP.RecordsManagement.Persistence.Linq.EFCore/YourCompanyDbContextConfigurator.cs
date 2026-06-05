using System;
using System.Collections.Generic;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore
{
    public abstract class YourCompanyDbContextConfigurator : Configuration.EFCore.YourCompanyDbContextConfigurator
    {
        public virtual IEnumerable<Type> EnumerateRecordTypes(
            YourCompanyDbContextConfiguratorsLoadingContext context) => null;

        public virtual IEnumerable<RecordDataQueryBuilder> EnumerateQueryBuilders(
            YourCompanyDbContextConfiguratorsLoadingContext context, Type recordDataType) => null;
    }
}