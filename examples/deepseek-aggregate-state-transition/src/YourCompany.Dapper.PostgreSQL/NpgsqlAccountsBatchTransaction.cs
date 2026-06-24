using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;

namespace YourCompany.Dapper.PostgreSQL
{
    internal sealed class NpgsqlAccountsBatchTransaction
        : DapperAccountsBatchTransaction
    {
        private readonly NpgsqlDataSource _dataSource;

        protected override IDapperAccountSqlBuilder SqlBuilder => NpgsqlAccountSqlBuilder.Instance;

        internal NpgsqlAccountsBatchTransaction(NpgsqlDataSource dataSource)
            => _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));

        protected override async Task<IDbTransaction> BeginTransaction(CancellationToken cancellationToken)
        {
            var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            try
            {
                return await connection.BeginTransactionAsync(cancellationToken);
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }
        }

        protected override async Task Commit(IDbTransaction transaction, CancellationToken cancellationToken)
        {
            if (transaction is not NpgsqlTransaction npgsqlTransaction)
                throw new ApplicationException("transaction is not NpgsqlTransaction");

            try
            {
                await npgsqlTransaction.CommitAsync(cancellationToken);
            }
            finally
            {
                await npgsqlTransaction.Connection.DisposeAsync();
            }
        }

        protected override async Task Rollback(IDbTransaction transaction, CancellationToken cancellationToken)
        {
            if (transaction is not NpgsqlTransaction npgsqlTransaction)
                throw new ApplicationException("transaction is not NpgsqlTransaction");

            try
            {
                await npgsqlTransaction.RollbackAsync(cancellationToken);
            }
            finally
            {
                await npgsqlTransaction.Connection.DisposeAsync();
            }
        }
    }
}