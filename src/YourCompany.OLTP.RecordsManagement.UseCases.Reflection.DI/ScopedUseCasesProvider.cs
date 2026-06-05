using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace YourCompany.OLTP.RecordsManagement.UseCases.Reflection.DI
{
    internal sealed class ScopedUseCasesProvider
    {
        private readonly IServiceProvider _provider;

        public ScopedUseCasesProvider(IServiceProvider provider) => _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        internal static void RegisterImplementedUseCases(UseCaseTypeInfo useCaseTypeInfo, IServiceCollection services)
        {
            if (useCaseTypeInfo == null) throw new ArgumentNullException(nameof(useCaseTypeInfo));
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (!useCaseTypeInfo.IsUseCasesImplementor) throw new ApplicationException("!useCaseTypeInfo.IsUseCasesImplementor");
            services.AddScoped(useCaseTypeInfo.Type);
            var useCases = useCaseTypeInfo.ImplementedUseCases ?? throw new ApplicationException("useCaseTypeInfo.ImplementedUseCases == null");
            foreach (var useCase in useCases)
                services.AddScoped(useCase, provider => provider.GetRequiredService(useCaseTypeInfo.Type));
        }

        internal IEnumerable<RecordsBatchTransactionCallback.IExtraInterfacesProvider>
            EnumerateRecordsBatchTransactionCallbackExtraInterfacesProviderIfAny()
        {
            if (_provider.GetService<RecordsBatchTransactionCallback.IExtraInterfacesProvider>() == null) return null;
            return _provider.GetServices<RecordsBatchTransactionCallback.IExtraInterfacesProvider>();
        }

        internal IEnumerable<RecordsBatchTransactionUseCases.IAuthorizer<TRecordData>>
            EnumerateRecordsBatchTransactionAuthorizersIfAny<TRecordData>()
            where TRecordData : class
        {
            if (_provider.GetService<RecordsBatchTransactionUseCases.IAuthorizer<TRecordData>>() == null) return null;
            return _provider.GetServices<RecordsBatchTransactionUseCases.IAuthorizer<TRecordData>>();
        }

        internal IEnumerable<RecordsBatchTransactionUseCases.IChangesPreparer>
            EnumerateRecordsBatchTransactionChangePreparersIfAny()
        {
            if (_provider.GetService<RecordsBatchTransactionUseCases.IChangesPreparer>() == null) return null;
            return _provider.GetServices<RecordsBatchTransactionUseCases.IChangesPreparer>();
        }

        internal IEnumerable<RecordsBatchTransactionUseCases.IChangesPreparer<TRecord>>
            EnumerateRecordsBatchTransactionChangePreparersIfAny<TRecord>()
            where TRecord : class
        {
            if (_provider.GetService<RecordsBatchTransactionUseCases.IChangesPreparer<TRecord>>() == null) return null;
            return _provider.GetServices<RecordsBatchTransactionUseCases.IChangesPreparer<TRecord>>();
        }

        internal IEnumerable<RecordsBatchTransactionUseCases.IFinishingHandler>
            EnumerateRecordsBatchTransactionFinishingHandlersIfAny()
        {
            if (_provider.GetService<RecordsBatchTransactionUseCases.IFinishingHandler>() == null) return null;
            return _provider.GetServices<RecordsBatchTransactionUseCases.IFinishingHandler>();
        }

        internal IEnumerable<RecordsBatchTransactionUseCases.IFinishingHandler<TRecord>>
            EnumerateRecordsBatchTransactionFinishingHandlersIfAny<TRecord>()
            where TRecord : class
        {
            if (_provider.GetService<RecordsBatchTransactionUseCases.IFinishingHandler<TRecord>>() == null) return null;
            return _provider.GetServices<RecordsBatchTransactionUseCases.IFinishingHandler<TRecord>>();
        }
    }
}