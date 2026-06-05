using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Extensions;
using YourCompany.OLTP.RecordsManagement;
using YourCompany.OLTP.RecordsManagement.DI.TransactionalComposition;
using YourCompany.OLTP.StateOwnership.TransactionalComposition.Reflection;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class YourCompanyServiceCollectionExtensions
    {
        public static IServiceCollection EnableYourCompanyDomainTypesTransactionalComposition(
            this IServiceCollection services, RecordTypesCompositionMap recordTypesCompositionMap)
        {
            if (recordTypesCompositionMap == null) throw new ArgumentNullException(nameof(recordTypesCompositionMap));
            if (!services.Any(x => x.ImplementationInstance == recordTypesCompositionMap.RecordTypesMap))
                throw new ApplicationException("!services.Any(x => x.ImplementationInstance == recordTypesCompositionMap.RecordTypesMap)");

            services.AddSingleton(recordTypesCompositionMap);
            services.AddScoped<ScopedRecordsBatchTransactionFactory>();

            foreach (var recordTypeInfo in recordTypesCompositionMap.Values)
            {
                if (!recordTypeInfo.IsRecord) continue;
                var exactRepositoryType = typeof(IRepository<>).MakeGenericType(recordTypeInfo.Type);
                if (services.Any(x => x.ServiceType == exactRepositoryType && x.ImplementationFactory != null))
                    throw new ApplicationException("services.Any(x => x.ServiceType == exactRepositoryType && x.ImplementationFactory != null)");

                services.RemoveAll(exactRepositoryType);
                services.AddScoped(exactRepositoryType, typeof(ScopedRepository<>).MakeGenericType(recordTypeInfo.Type));
            }

            return services;
        }

        public static IServiceCollection OverrideYourCompanyScopedRepositoryRecordsDataAccessProvider<TRecord>(
            this IServiceCollection services,
            Func<IServiceProvider, IComposableRecordsDataAccessProvider> recordsDataAccessProviderFactory)
            where TRecord : class
        {
            if (recordsDataAccessProviderFactory == null) throw new ArgumentNullException(nameof(recordsDataAccessProviderFactory));
            if (!services.Any(x => x.ServiceType == typeof(IRepository<>)))
                throw new ApplicationException("!services.Any(x => x.ServiceType == typeof(IRepository<>))");
            if (services.Any(x => x.ServiceType == typeof(IRepository<TRecord>)))
                throw new ApplicationException("services.Any(x => x.ServiceType == typeof(IRepository<TRecord>))");

            return services.AddScoped<IRepository<TRecord>>(provider => new ScopedRepository<TRecord>(
                provider.GetRequiredService<ScopedRecordsBatchTransactionFactory>(),
                recordsDataAccessProviderFactory(provider)));
        }
    }
}