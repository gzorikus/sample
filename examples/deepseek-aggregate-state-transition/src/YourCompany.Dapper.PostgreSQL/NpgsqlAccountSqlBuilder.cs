using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using YourCompany.OLTP.StateOwnership;
using YourCompany.Persistence;

namespace YourCompany.Dapper.PostgreSQL
{
    internal sealed class NpgsqlAccountSqlBuilder : IDapperAccountSqlBuilder
    {
        public static IDapperAccountSqlBuilder Instance { get; } = new NpgsqlAccountSqlBuilder();

        private NpgsqlAccountSqlBuilder() { }

        public void Build(IDbCommand command, IReadOnlyList<DapperAccountState> states)
        {
            if (command is not NpgsqlCommand npgsqlCommand)
                throw new ApplicationException("command is not NpgsqlCommand");

            string jsonbSourceSql = NpgsqlAccountSqlHelper.BuildJsonbChangesArraySourceSql(
                states,
                npgsqlCommand.Parameters);

            npgsqlCommand.CommandText = NpgsqlAccountSqlHelper.BuildFullCte(jsonbSourceSql);
        }

        public void Build(
            IDbCommand command,
            IReadOnlyList<ISpecification<IAccountRecordData>> batchConditions,
            IReadOnlyList<ISpecification<IAccountRecordData>> batchActions,
            int batchSize,
            bool useForUpdateSkipLocked)
        {
            if (command is not NpgsqlCommand npgsqlCommand)
                throw new ApplicationException("command is not NpgsqlCommand");

            string jsonbSourceSql = NpgsqlAccountSqlHelper.BuildJsonbQuerySourceSql(
                batchActions,
                batchConditions,
                useForUpdateSkipLocked,
                batchSize,
                npgsqlCommand.Parameters);

            npgsqlCommand.CommandText = NpgsqlAccountSqlHelper.BuildFullCte(jsonbSourceSql);
        }
    }
}