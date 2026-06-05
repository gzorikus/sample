using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using YourCompany.Reflection.DI;

namespace YourCompany.OLTP.StateOwnership.Reflection.DI
{
    internal sealed class ScopedRecordsProvider
    {
        private readonly IServiceProvider _provider;

        private static readonly AsyncLocal<ICurrentStateAccessProvider> CurrentStateAccessProvider = new();

        public ScopedRecordsProvider(IServiceProvider provider) => _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        internal static void RegisterRecord(RecordTypeInfo recordTypeInfo, IServiceCollection services)
        {
            if (recordTypeInfo == null) throw new ArgumentNullException(nameof(recordTypeInfo));
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (!recordTypeInfo.IsRecord) throw new ApplicationException("!recordTypeInfo.IsRecord");

            var recordFactory = DIConstructorHelper.CompileRequiredArgumentsFactory(recordTypeInfo.ConstructorInfo);

            services.AddTransient(recordTypeInfo.Type, provider =>
            {
                var currentStateAccess = GetCurrentStateAccess(recordTypeInfo.RecordDataType);
                if (!currentStateAccess.TryGetRecordObject(out object recordObject))
                {
                    if (recordTypeInfo.CheckRecordHasConstructionLimitationReason(typeof(RecordConstructionException)))
                        throw recordTypeInfo.GetRecordConstructionExceptionForThrowing();

                    recordObject = recordFactory(provider);
                    currentStateAccess.SetRecordObject(recordObject);
                }
                return recordObject ?? throw new ApplicationException("recordObject == null");
            });
        }

        internal static void StartRecordCreation(ICurrentStateAccessProvider stateAccessProvider)
        {
            if (stateAccessProvider == null) throw new ArgumentNullException(nameof(stateAccessProvider));
            if (CurrentStateAccessProvider.Value != null) throw new ApplicationException("CurrentStateAccessProvider != null");
            CurrentStateAccessProvider.Value = stateAccessProvider;
        }

        internal static void AssertCurrentStateAccessProvider(ICurrentStateAccessProvider stateAccessProvider)
        {
            if (stateAccessProvider == null) throw new ArgumentNullException(nameof(stateAccessProvider));
            if (CurrentStateAccessProvider.Value != stateAccessProvider) throw new ApplicationException("CurrentStateAccessProvider.Value != stateAccessProvider");
        }

        internal TRecord ResolveRecord<TRecord>() where TRecord : class => _provider.GetRequiredService<TRecord>();

        internal static IState<TRecordData> ExchangeTransactionCallbackForState<TRecordData>(
            EventHandler transactionCallback)
            where TRecordData : class
        {
            if (transactionCallback == null) throw new ArgumentNullException(nameof(transactionCallback));
            var currentStateAccess = GetCurrentStateAccess(typeof(TRecordData));
            return transactionCallback == SingletonStateAccess<TRecordData>.ReadOnlyTransactionDoesNotRequireCallback
                && !currentStateAccess.ReadOnly
                ? null
                : currentStateAccess.ExchangeTransactionCallbackForState<TRecordData>(transactionCallback);
        }

        internal static void StopRecordCreation(ICurrentStateAccessProvider stateAccessProvider)
        {
            if (stateAccessProvider == null) throw new ArgumentNullException(nameof(stateAccessProvider));
            if (CurrentStateAccessProvider.Value != stateAccessProvider) throw new ApplicationException("CurrentStateAccessProvider.Value != stateAccessProvider");
            CurrentStateAccessProvider.Value = null;
        }

        private static ICurrentStateAccess GetCurrentStateAccess(Type recordDataType)
        {
            var stateAccessProvider = CurrentStateAccessProvider.Value ?? throw new ApplicationException("CurrentStateAccessProvider.Value == null");
            var currentStateAccess = stateAccessProvider.GetCurrentStateAccess(recordDataType);
            return currentStateAccess ?? throw new ApplicationException("currentStateAccess == null");
        }
    }
}