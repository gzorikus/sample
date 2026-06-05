using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YourCompany.OLTP.RecordsManagement.Persistence;
using YourCompany.OLTP.RecordsManagement.UseCases;
using YourCompany.OLTP.StateOwnership;
using YourCompany.OLTP.StateOwnership.Reflection.DI;

namespace YourCompany.OLTP.RecordsManagement.DI
{
    internal abstract partial class RecordsBatchTransaction<TRecord, TRecordData>
    {
        internal abstract partial class CurrentStateAccess : RecordsBatchTransaction<TRecord, TRecordData>,
            ICurrentStateAccess
        {
            private ScopedRecordsBatchTransactionFactory _recordsBatchTransactionFactory;
            private CurrentlyResolvedRecord? _currentlyResolvedRecord;

            bool ICurrentStateAccess.ReadOnly => ReadOnly;

            private CurrentStateAccess(
                ScopedRecordsBatchTransactionFactory recordsBatchTransactionFactory,
                RecordsDataAccess.IStarting recordsDataAccess)
                : base(recordsDataAccess)
                => _recordsBatchTransactionFactory = recordsBatchTransactionFactory ?? throw new ArgumentNullException(nameof(recordsBatchTransactionFactory));

            protected override IEnumerable<RecordsBatchTransactionCallback.IExtraInterfacesProvider>
                EnumerateRecordsBatchTransactionCallbackExtraInterfacesProviderIfAny()
            {
                if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                if (_currentlyResolvedRecord.HasValue) throw new ApplicationException("_currentlyResolvedRecord.HasValue");
                if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                CurrentRunState.EnsureIsAfterConfiguration();
                CurrentRunState.EnsureIsBeforeReturning();
                return _recordsBatchTransactionFactory.GetUseCases()
                    .EnumerateRecordsBatchTransactionCallbackExtraInterfacesProviderIfAny();
            }

            protected override IEnumerable<RecordsBatchTransactionUseCases.IAuthorizer<TRecordData>>
                EnumerateRecordsBatchTransactionAuthorizersIfAny()
            {
                if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                if (_currentlyResolvedRecord.HasValue) throw new ApplicationException("_currentlyResolvedRecord.HasValue");
                CurrentRunState.EnsureIsStarted();
                return _recordsBatchTransactionFactory.GetUseCases()
                    .EnumerateRecordsBatchTransactionAuthorizersIfAny<TRecordData>();
            }

            protected override BuiltRecord BuildRecord(int recordsCount, int recordIndex, Identity identity)
            {
                if (identity == null) throw new ArgumentNullException(nameof(identity));
                if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                if (_currentlyResolvedRecord.HasValue) throw new ApplicationException("_currentlyResolvedRecord.HasValue");
                if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                CurrentRunState.EnsureIsRecordsBuildingStarted();

                _currentlyResolvedRecord = new CurrentlyResolvedRecord(recordIndex, identity);
                try
                {
                    var record = _recordsBatchTransactionFactory.ResolveRecord<TRecord>() ?? throw new ApplicationException("record == null");
                    if (!ReferenceEquals(record, _currentlyResolvedRecord.Value.Record)) throw new ApplicationException("!ReferenceEquals(record, _currentlyResolvedRecord.Value.Record)");
                    return new BuiltRecord(
                        _currentlyResolvedRecord.Value.TransactionCallback ?? throw new ApplicationException("_currentlyResolvedRecord.Value.TransactionCallback == null"),
                        _currentlyResolvedRecord.Value.State ?? throw new ApplicationException("_currentlyResolvedRecord.Value.State == null"),
                        record);
                }
                finally
                {
                    _currentlyResolvedRecord = default;
                }
            }

            bool ICurrentStateAccess.TryGetRecordObject(out object recordObject)
            {
                if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                if (!_currentlyResolvedRecord.HasValue) throw new ApplicationException("!_currentlyResolvedRecord.HasValue");
                if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                CurrentRunState.EnsureIsRecordsBuildingStarted();
                recordObject = _currentlyResolvedRecord.Value.Record;
                return recordObject != null;
            }

            IState<TAccessedRecordData> ICurrentStateAccess.ExchangeTransactionCallbackForState<TAccessedRecordData>(
                EventHandler transactionCallback)
                where TAccessedRecordData : class
            {
                if (typeof(TAccessedRecordData) != typeof(TRecordData)) throw new ApplicationException("typeof(TAccessedRecordData) != typeof(TRecordData)");
                if (transactionCallback == null) throw new ArgumentNullException(nameof(transactionCallback));
                if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                if (!_currentlyResolvedRecord.HasValue) throw new ApplicationException("!_currentlyResolvedRecord.HasValue");
                if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                CurrentRunState.EnsureIsRecordsBuildingStarted();
                int currentRecordIndex = _currentlyResolvedRecord.Value.RecordIndex ?? throw new ApplicationException("_currentlyResolvedRecord.Value.RecordIndex.HasValue");
                var identity = _currentlyResolvedRecord.Value.Identity ?? throw new ApplicationException("_currentlyResolvedRecord.Value.Identity == null");
                var state = new RecordState<TRecordData>(this, currentRecordIndex, identity);
                _currentlyResolvedRecord = _currentlyResolvedRecord.Value.WithStateExchanged(transactionCallback, state);
                return (IState<TAccessedRecordData>)(IState<TRecordData>)state;
            }

            void ICurrentStateAccess.SetRecordObject(object recordObject)
            {
                if (recordObject == null) throw new ArgumentNullException(nameof(recordObject));
                if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                if (!_currentlyResolvedRecord.HasValue) throw new ApplicationException("!_currentlyResolvedRecord.HasValue");
                if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                CurrentRunState.EnsureIsRecordsBuildingStarted();
                _currentlyResolvedRecord = _currentlyResolvedRecord.Value.WithRecordResolved((TRecord)recordObject);
            }

            protected override IEnumerable<RecordsBatchTransactionUseCases.IChangesPreparer>
                EnumerateRecordsBatchTransactionChangePreparersWithoutRecordsIfAny()
            {
                if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                if (_currentlyResolvedRecord.HasValue) throw new ApplicationException("_currentlyResolvedRecord.HasValue");
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                CurrentRunState.EnsureIsRecordsBuildingStarted();
                return _recordsBatchTransactionFactory.GetUseCases()
                    .EnumerateRecordsBatchTransactionChangePreparersIfAny();
            }

            protected override IEnumerable<RecordsBatchTransactionUseCases.IChangesPreparer<TRecord>>
                EnumerateRecordsBatchTransactionChangePreparersWithRecordsIfAny()
            {
                if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                if (_currentlyResolvedRecord.HasValue) throw new ApplicationException("_currentlyResolvedRecord.HasValue");
                if (ReadOnly) throw new ApplicationException("ReadOnly");
                CurrentRunState.EnsureIsRecordsBuildingStarted();
                return _recordsBatchTransactionFactory.GetUseCases()
                    .EnumerateRecordsBatchTransactionChangePreparersIfAny<TRecord>();
            }

            protected override IEnumerable<RecordsBatchTransactionUseCases.IFinishingHandler>
                EnumerateRecordsBatchTransactionFinishingHandlersWithoutRecordsIfAny()
            {
                if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                if (_currentlyResolvedRecord.HasValue) throw new ApplicationException("_currentlyResolvedRecord.HasValue");
                if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                CurrentRunState.EnsureIsResultingRecordsBatchHandling();
                return _recordsBatchTransactionFactory.GetUseCases()
                    .EnumerateRecordsBatchTransactionFinishingHandlersIfAny();
            }

            protected override IEnumerable<RecordsBatchTransactionUseCases.IFinishingHandler<TRecord>>
                EnumerateRecordsBatchTransactionFinishingHandlersWithRecordsIfAny()
            {
                if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                if (_currentlyResolvedRecord.HasValue) throw new ApplicationException("_currentlyResolvedRecord.HasValue");
                if (ReadOnly && !ReadOnlyIncludeRecords) throw new ApplicationException("ReadOnly && !ReadOnlyIncludeRecords");
                CurrentRunState.EnsureIsResultingRecordsBatchHandling();
                return _recordsBatchTransactionFactory.GetUseCases()
                    .EnumerateRecordsBatchTransactionFinishingHandlersIfAny<TRecord>();
            }

            protected override Task Return(CancellationToken cancellationToken, Exception runException = null)
            {
                if (_recordsBatchTransactionFactory == null) throw new ApplicationException("_recordsBatchTransactionFactory == null");
                if (_currentlyResolvedRecord.HasValue) throw new ApplicationException("_currentlyResolvedRecord.HasValue");
                _recordsBatchTransactionFactory = null;
                return base.Return(cancellationToken, runException);
            }
        }
    }
}