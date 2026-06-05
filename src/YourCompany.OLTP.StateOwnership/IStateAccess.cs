using System;

namespace YourCompany.OLTP.StateOwnership
{
    public interface IStateAccess<TRecordData> where TRecordData : class
    {
        IState<TRecordData> ReadOnly { get; }
        IState<TRecordData> Claim(EventHandler transactionCallback);
    }
}