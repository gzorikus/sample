using System;

namespace YourCompany.OLTP.StateOwnership.Reflection.DI
{
    internal interface ICurrentStateAccess
    {
        bool ReadOnly { get; }
        bool TryGetRecordObject(out object recordObject);
        IState<TRecordData> ExchangeTransactionCallbackForState<TRecordData>(EventHandler transactionCallback) where TRecordData : class;
        void SetRecordObject(object recordObject);
    }
}