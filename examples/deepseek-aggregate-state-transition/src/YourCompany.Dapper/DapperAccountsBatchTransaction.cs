using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using YourCompany.OLTP.StateOwnership;
using YourCompany.Persistence;

namespace YourCompany.Dapper
{
    internal abstract class DapperAccountsBatchTransaction
        : IAsyncDisposable, IStateAccess<IAccountRecordData>, SqlMapper.IDynamicParameters
    {
        internal const int MaxBatchSize = 50;

        private readonly List<Account> _accounts;
        private List<ISpecification<IAccountRecordData>> _batchConditions;
        private List<ISpecification<IAccountRecordData>> _batchActions;
        private DapperAccountId _pendingAccountId;
        private List<DapperAccountState> _registeredStates;
        private bool _disposed;

        internal int BatchSize { get; private set; } = MaxBatchSize;
        internal bool UseForUpdateSkipLocked { get; private set; }
        internal IReadOnlyList<Account> Accounts => _accounts;

        protected DapperAccountsBatchTransaction()
        {
            _accounts = new List<Account>();
        }

        protected abstract IDapperAccountSqlBuilder SqlBuilder { get; }

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

        internal async Task Run(CancellationToken cancellationToken = default)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DapperAccountsBatchTransaction));

            await using var _ = this;

            IDbTransaction transaction = null;
            try
            {
                transaction = await BeginTransaction(cancellationToken);
                var connection = transaction.Connection ?? throw new ApplicationException("transaction.Connection == null");

                var command = new CommandDefinition(
                    commandText: "see AddParameters 👇",
                    parameters: this,
                    transaction: transaction,
                    cancellationToken: cancellationToken);
                var rows = await connection.QueryAsync<DapperAccountRecordData>(command);

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

                await Commit(transaction, cancellationToken);
                transaction = null;
            }
            finally
            {
                if (transaction != null)
                    await Rollback(transaction, cancellationToken);
            }
        }

        protected abstract Task<IDbTransaction> BeginTransaction(CancellationToken cancellationToken);
        protected abstract Task Commit(IDbTransaction transaction, CancellationToken cancellationToken);
        protected abstract Task Rollback(IDbTransaction transaction, CancellationToken cancellationToken);

        void SqlMapper.IDynamicParameters.AddParameters(IDbCommand command, SqlMapper.Identity identity)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(DapperAccountsBatchTransaction));

            command.CommandText = string.Empty;

            if (_registeredStates != null)
            {
                SqlBuilder.Build(command, _registeredStates);
            }
            else
            {
                IReadOnlyList<ISpecification<IAccountRecordData>> before = _batchConditions;
                IReadOnlyList<ISpecification<IAccountRecordData>> after = _batchActions;
                SqlBuilder.Build(
                    command,
                    before ?? Array.Empty<ISpecification<IAccountRecordData>>(),
                    after ?? Array.Empty<ISpecification<IAccountRecordData>>(),
                    BatchSize,
                    UseForUpdateSkipLocked);
            }

            if (string.IsNullOrEmpty(command.CommandText))
                throw new ApplicationException("string.IsNullOrEmpty(command.CommandText)");
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
            await DisposeAsync(true);
            GC.SuppressFinalize(this);
        }

        protected virtual ValueTask DisposeAsync(bool disposing)
        {
            if (!_disposed) _disposed = true;
            return ValueTask.CompletedTask;
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
    }
}