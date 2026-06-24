using System.Collections.Generic;
using System.Data;
using YourCompany.OLTP.StateOwnership;
using YourCompany.Persistence;

namespace YourCompany.Dapper
{
    internal interface IDapperAccountSqlBuilder
    {
        void Build(
            IDbCommand command,
            IReadOnlyList<DapperAccountState> states);

        void Build(
            IDbCommand command,
            IReadOnlyList<ISpecification<IAccountRecordData>> batchConditions,
            IReadOnlyList<ISpecification<IAccountRecordData>> batchActions,
            int batchSize,
            bool useForUpdateSkipLocked);
    }
}