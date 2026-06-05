using System;

namespace YourCompany.OLTP.StateOwnership.Reflection.DI
{
    internal sealed class SingletonStateAccess<TRecordData> : IStateAccess<TRecordData> where TRecordData : class
    {
        internal static EventHandler ReadOnlyTransactionDoesNotRequireCallback { get; } = (_, _) => { };

        public IState<TRecordData> ReadOnly
            => ScopedRecordsProvider.ExchangeTransactionCallbackForState<TRecordData>(
                ReadOnlyTransactionDoesNotRequireCallback);

        public IState<TRecordData> Claim(EventHandler transactionCallback)
        {
            if (transactionCallback == null) throw new ArgumentNullException(nameof(transactionCallback));
            return ScopedRecordsProvider.ExchangeTransactionCallbackForState<TRecordData>(transactionCallback);
        }
    }
}