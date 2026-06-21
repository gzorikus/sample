using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using YourCompany.OLTP.StateOwnership;
using YourCompany.Persistence;

namespace YourCompany.Dapper
{
    internal sealed class DapperAccountsBatchTransaction
        : IAsyncDisposable, IStateAccess<IAccountRecordData>, SqlMapper.IDynamicParameters
    {
        internal const int MaxBatchSize = 50;

        private readonly NpgsqlDataSource _dataSource;
        private readonly List<Account> _accounts;

        private List<ISpecification<IAccountRecordData>> _batchConditions;
        private List<ISpecification<IAccountRecordData>> _batchActions;
        private DapperAccountId _pendingAccountId;
        private List<DapperAccountState> _registeredStates;
        private NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction;
        private bool _disposed;

        internal int BatchSize { get; private set; } = MaxBatchSize;
        internal bool UseForUpdateSkipLocked { get; private set; }
        internal IReadOnlyList<Account> Accounts => _accounts;

        internal DapperAccountsBatchTransaction(NpgsqlDataSource dataSource)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _accounts = new List<Account>();
        }

        internal void ConfigureBatch(int batchSize, bool useForUpdateSkipLocked = false)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DapperAccountsBatchTransaction));
            if (_registeredStates != null) throw new ApplicationException("_registeredStates != null");
            if (batchSize <= 0 || batchSize > MaxBatchSize)
                throw new ArgumentOutOfRangeException(nameof(batchSize), batchSize, message: null);
            BatchSize = batchSize;
            UseForUpdateSkipLocked = useForUpdateSkipLocked;
        }

        internal void MatchBeforeDataChanging(ISpecification<IAccountRecordData> specification)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DapperAccountsBatchTransaction));
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (_registeredStates != null) throw new ApplicationException("_registeredStates != null");
            _batchConditions ??= new List<ISpecification<IAccountRecordData>>();
            _batchConditions.Add(specification);
        }

        internal void ChangeDataToMatch(ISpecification<IAccountRecordData> specification)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DapperAccountsBatchTransaction));
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (_registeredStates != null) throw new ApplicationException("_registeredStates != null");
            _batchActions ??= new List<ISpecification<IAccountRecordData>>();
            _batchActions.Add(specification);
        }

        internal Account ById(DapperAccountId id)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DapperAccountsBatchTransaction));
            if (UseForUpdateSkipLocked) throw new ApplicationException("UseForUpdateSkipLocked");
            return TrackAccount(id);
        }

        internal async Task Run()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DapperAccountsBatchTransaction));

            await using var _ = this;

            await Connect();

            try
            {
                var rows = await _connection.QueryAsync<DapperAccountRecordData>(
                    sql: "see AddParameters 👇👉", param: this, _transaction);

                if (_registeredStates != null)
                {
                    if (_accounts.Count != _registeredStates.Count)
                        throw new ApplicationException("_accounts.Count != _registeredStates.Count");

                    var resultDictById = new Dictionary<long, DapperAccountRecordData>();
                    var resultDictByPublicId = new Dictionary<Guid, DapperAccountRecordData>();

                    foreach (var row in rows)
                    {
                        if (!row.ResolvedId.HasValue || !row.ResolvedPublicId.HasValue)
                            throw new ApplicationException("!row.ResolvedId.HasValue || !row.ResolvedPublicId.HasValue");

                        resultDictById.Add(row.ResolvedId.Value, row);
                        resultDictByPublicId.Add(row.ResolvedPublicId.Value, row);
                    }

                    if (resultDictById.Count != _registeredStates.Count || resultDictByPublicId.Count != _registeredStates.Count)
                        throw new ApplicationException("resultDictById.Count != _registeredStates.Count || resultDictByPublicId.Count != _registeredStates.Count");

                    for (int i = 0; i < _registeredStates.Count; i++)
                    {
                        var state = _registeredStates[i];
                        DapperAccountRecordData row;
                        if (state.Identity.HasValue)
                        {
                            if (!resultDictById.TryGetValue(state.Identity.Value, out row))
                                throw new ApplicationException("!resultDictById.ContainsKey(state.Identity.Value)");
                        }
                        else if (state.Identity.HasPublicId)
                        {
                            if (!resultDictByPublicId.TryGetValue(state.Identity.PublicId, out row))
                                throw new ApplicationException("!resultDictByPublicId.ContainsKey(state.Identity.PublicId)");
                        }
                        else
                        {
                            throw new ApplicationException("!state.Identity.HasValue && !state.Identity.HasPublicId");
                        }

                        state.SetLockedData(row);
                        if (!state.Identity.HasValue)
                            state.Identity.Value = row.ResolvedId.Value;
                        if (!state.Identity.HasPublicId)
                            state.Identity.PublicId = row.ResolvedPublicId.Value;
                        if (state.Identity.HasPublicId && state.Identity.PublicId != row.ResolvedPublicId.Value)
                            throw new ApplicationException("state.Identity.PublicId != row.ResolvedPublicId");

                        if (state.Identity.CheckExpectsAssignment()) throw new ApplicationException("state.Identity.CheckExpectsAssignment()");
                    }
                }
                else
                {
                    foreach (var row in rows)
                    {
                        if (!row.ResolvedId.HasValue || !row.ResolvedPublicId.HasValue)
                            throw new ApplicationException("!row.ResolvedId.HasValue || !row.ResolvedPublicId.HasValue");

                        var account = TrackAccount(new DapperAccountId(row.ResolvedPublicId.Value));
                        var state = _registeredStates?.LastOrDefault() ?? throw new ApplicationException("_registeredStates?.LastOrDefault() == null");
                        if (state.Identity.PublicId != row.ResolvedPublicId.Value)
                            throw new ApplicationException("state.Identity.PublicId != row.ResolvedPublicId");

                        state.SetLockedData(row);
                        state.Identity.Value = row.ResolvedId.Value;
                        if (state.Identity.CheckExpectsAssignment()) throw new ApplicationException("state.Identity.CheckExpectsAssignment()");
                    }
                }

                if (_registeredStates != null)
                {
                    for (int i = 0; i < _registeredStates.Count; i++) _registeredStates[i].OnAfterRecordDataLocking();
                    for (int i = 0; i < _registeredStates.Count; i++) _registeredStates[i].OnAfterDataAccess();
                }

                await _transaction.CommitAsync();
            }
            catch
            {
                if (_transaction != null) await _transaction.RollbackAsync();
                throw;
            }
        }

        void SqlMapper.IDynamicParameters.AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DapperAccountsBatchTransaction));
            if (_connection == null || _transaction == null) throw new ApplicationException("_connection == null || _transaction == null");
            if (command is not NpgsqlCommand npgsqlCommand) throw new ApplicationException("command is not NpgsqlCommand npgsqlCommand");

            string jsonbSourceSql;

            if (_registeredStates != null)
            {
                jsonbSourceSql = NpgsqlAccountSqlHelper.BuildJsonbChangesArraySourceSql(_registeredStates, npgsqlCommand.Parameters);
            }
            else
            {
                IReadOnlyList<ISpecification<IAccountRecordData>> before = _batchConditions;
                IReadOnlyList<ISpecification<IAccountRecordData>> after = _batchActions;
                jsonbSourceSql = NpgsqlAccountSqlHelper.BuildJsonbQuerySourceSql(
                    batchActions: after ?? Array.Empty<ISpecification<IAccountRecordData>>(),
                    batchConditions: before ?? Array.Empty<ISpecification<IAccountRecordData>>(),
                    UseForUpdateSkipLocked,
                    BatchSize,
                    npgsqlCommand.Parameters);
            }

            npgsqlCommand.CommandText = NpgsqlAccountSqlHelper.BuildFullCte(jsonbSourceSql);
        }

        IState<IAccountRecordData> IStateAccess<IAccountRecordData>.ReadOnly => null;

        IState<IAccountRecordData> IStateAccess<IAccountRecordData>.Claim(EventHandler transactionCallback)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DapperAccountsBatchTransaction));
            if (transactionCallback == null) throw new ArgumentNullException(nameof(transactionCallback));

            if (_pendingAccountId == null)
                throw new ApplicationException("_pendingAccountId == null");

            var newState = new DapperAccountState(_pendingAccountId, transactionCallback);
            _pendingAccountId = null;

            _registeredStates ??= new List<DapperAccountState>();
            _registeredStates.Add(newState);

            return newState;
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await Disconnect();
                _registeredStates = null;
                _batchConditions = null;
                _batchActions = null;
                _disposed = true;
            }
        }

        private Account TrackAccount(DapperAccountId id)
        {
            if (_pendingAccountId != null) throw new ApplicationException("_pendingAccountId != null");
            if (_accounts.Count >= BatchSize) throw new ApplicationException("_accounts.Count >= BatchSize");

            _pendingAccountId = id ?? throw new ArgumentNullException(nameof(id));
            _registeredStates ??= new List<DapperAccountState>();
            int countBefore = _registeredStates.Count;

            var account = new Account(this);
            _accounts.Add(account);
            if (_registeredStates.Count != countBefore + 1) throw new ApplicationException("_registeredStates.Count != countBefore + 1");

            var newState = _registeredStates[countBefore];

            IReadOnlyList<ISpecification<IAccountRecordData>> before = _batchConditions;
            IReadOnlyList<ISpecification<IAccountRecordData>> after = _batchActions;
            newState.SetBatchSpecifications(
                before ?? Array.Empty<ISpecification<IAccountRecordData>>(),
                after ?? Array.Empty<ISpecification<IAccountRecordData>>());

            newState.OnBeforeDataChanging();
            newState.TriggerBatchSpecificationsCallbacks();

            return account;
        }

        private async Task Connect()
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DapperAccountsBatchTransaction));
            if (_connection != null) throw new ApplicationException("_connection != null");
            if (_transaction != null) throw new ApplicationException("_transaction != null");
            _connection = await _dataSource.OpenConnectionAsync();
            _transaction = await _connection.BeginTransactionAsync();
        }

        private async Task Disconnect()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
    }
}