using System;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        internal abstract partial class WithRecords<TRecord>
        {
            internal abstract partial class WithRecordsData<TRecordData>
            {
                internal readonly struct BuiltRecord
                {
                    internal EventHandler TransactionCallback { get; }
                    internal RecordState<TRecordData> State { get; }
                    internal TRecord Record { get; }
                    internal bool BeforeDataChangingAssertionWasTriggered { get; }

                    internal BuiltRecord(EventHandler transactionCallback, RecordState<TRecordData> state, TRecord record)
                    {
                        TransactionCallback = transactionCallback ?? throw new ArgumentNullException(nameof(transactionCallback));
                        State = state ?? throw new ArgumentNullException(nameof(state));
                        Record = record ?? throw new ArgumentNullException(nameof(record));
                        BeforeDataChangingAssertionWasTriggered = false;
                    }

                    private BuiltRecord(BuiltRecord builtRecord, bool beforeDataChangingAssertionWasTriggered = false)
                    {
                        if (!builtRecord.CheckRecordWasBuilt()) throw new ApplicationException("!builtRecord.CheckRecordWasBuilt()");
                        TransactionCallback = builtRecord.TransactionCallback;
                        State = builtRecord.State;
                        Record = builtRecord.Record;
                        BeforeDataChangingAssertionWasTriggered = builtRecord.BeforeDataChangingAssertionWasTriggered
                            || beforeDataChangingAssertionWasTriggered;
                    }

                    internal BuiltRecord WithBeforeDataChangingAssertionTriggered()
                        => new BuiltRecord(this, beforeDataChangingAssertionWasTriggered: true);

                    internal bool CheckRecordWasBuilt()
                    {
                        bool stateWasClaimed = CheckTransactionCallbackWasExchangedForState();
                        bool recordWasBuilt = Record != null;
                        if (recordWasBuilt && !stateWasClaimed) throw new ApplicationException("recordWasBuilt && !stateWasClaimed");
                        return recordWasBuilt;
                    }

                    internal bool CheckTransactionCallbackWasExchangedForState()
                    {
                        bool transactionCallbackWaExchanged = TransactionCallback != null;
                        bool stateWasClaimed = State != null;
                        if (transactionCallbackWaExchanged != stateWasClaimed) throw new ApplicationException("transactionCallbackWaExchanged != stateWasClaimed");
                        return stateWasClaimed;
                    }
                }
            }
        }
    }
}