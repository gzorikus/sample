using System;
using System.Collections.Generic;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq
{
    public interface IRecordDataQueryBuildersProvider
    {
        IReadOnlyList<RecordDataQueryBuilder> GetQueryBuilders(Type recordDataType);
    }
}