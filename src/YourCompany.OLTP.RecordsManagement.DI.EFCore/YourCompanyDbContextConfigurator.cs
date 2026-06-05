using System;
using System.Collections.Generic;
using YourCompany.OLTP.RecordsManagement.Persistence.Linq;
using YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore;

namespace YourCompany.OLTP.RecordsManagement.DI.EFCore
{
    public abstract class YourCompanyDbContextConfigurator : Persistence.Linq.EFCore.YourCompanyDbContextConfigurator
    {
        public virtual IEnumerable<RecordDataQueryBuilder> EnumerateScopedQueryBuilders(
            YourCompanyDbContextConfiguratorsLoadingContext context, Type recordDataType, IServiceProvider provider)
            => null;
    }
}