using System;
using YourCompany.OLTP.StateOwnership;

namespace YourCompany.OLTP.RecordsManagement.DI
{
    internal abstract partial class RecordsBatchTransaction<TRecord, TRecordData>
    {
        private readonly struct CurrentlyResolvedRecord
        {
            internal int? RecordIndex { get; }
            internal Identity Identity { get; }
            internal EventHandler TransactionCallback { get; }
            internal RecordState<TRecordData> State { get; }
            internal TRecord Record { get; }

            internal CurrentlyResolvedRecord(int recordIndex, Identity identity) : this()
            {
                if (recordIndex < 0) throw new ArgumentOutOfRangeException(nameof(recordIndex), recordIndex, message: null);
                RecordIndex = recordIndex;
                Identity = identity ?? throw new ArgumentNullException(nameof(identity));
            }

            private CurrentlyResolvedRecord(CurrentlyResolvedRecord resolving, EventHandler transactionCallback, RecordState<TRecordData> state)
            {
                if (resolving.TransactionCallback != null) throw new ApplicationException("resolving.TransactionCallback != null");
                if (resolving.State != null) throw new ApplicationException("resolving.State != null");
                if (resolving.Record != null) throw new ApplicationException("resolving.Record != null");
                RecordIndex = resolving.RecordIndex ?? throw new ApplicationException("!resolving.RecordIndex.HasValue");
                Identity = resolving.Identity ?? throw new ApplicationException("resolving.Identity == null");
                TransactionCallback = transactionCallback ?? throw new ArgumentNullException(nameof(transactionCallback));
                State = state ?? throw new ArgumentNullException(nameof(state));
                Record = null;
            }

            private CurrentlyResolvedRecord(CurrentlyResolvedRecord stateExchanged, TRecord record)
            {
                if (stateExchanged.Record != null) throw new ApplicationException("stateExchanged.Record != null");
                RecordIndex = stateExchanged.RecordIndex ?? throw new ApplicationException("!stateExchanged.RecordIndex.HasValue");
                Identity = stateExchanged.Identity ?? throw new ApplicationException("stateExchanged.Identity == null");
                TransactionCallback = stateExchanged.TransactionCallback ?? throw new ApplicationException("stateExchanged.TransactionCallback == null");
                State = stateExchanged.State ?? throw new ApplicationException("stateExchanged.State == null");
                Record = record ?? throw new ArgumentNullException(nameof(record));
            }

            internal CurrentlyResolvedRecord WithStateExchanged(
                EventHandler transactionCallback, RecordState<TRecordData> state)
                => new(this, transactionCallback, state);

            internal CurrentlyResolvedRecord WithRecordResolved(TRecord record) => new(this, record);
        }
    }
}