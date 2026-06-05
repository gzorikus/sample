using System;

namespace YourCompany.OLTP.RecordsManagement
{
    public static partial class RecordsBatchTransaction
    {
        public abstract partial class WithRecords<TRecord>
        {
            public abstract partial class WithRecordsData<TRecordData>
            {
                protected readonly struct BuiltRecord
                {
                    public EventHandler TransactionCallback { get; }
                    public RecordState<TRecordData> State { get; }
                    public TRecord Record { get; }
                    public bool BeforeDataChangingAssertionWasTriggered { get; }

                    public BuiltRecord(EventHandler transactionCallback, RecordState<TRecordData> state, TRecord record)
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

                    public BuiltRecord WithBeforeDataChangingAssertionTriggered()
                        => new BuiltRecord(this, beforeDataChangingAssertionWasTriggered: true);

                    public bool CheckRecordWasBuilt()
                    {
                        bool stateWasClaimed = CheckTransactionCallbackWasExchangedForState();
                        bool recordWasBuilt = Record != null;
                        if (recordWasBuilt && !stateWasClaimed) throw new ApplicationException("recordWasBuilt && !stateWasClaimed");
                        return recordWasBuilt;
                    }

                    public bool CheckTransactionCallbackWasExchangedForState()
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