using System;
using System.Linq;
using YourCompany.OLTP.RecordsManagement;
using YourCompany.OLTP.RecordsManagement.DI;
using YourCompany.OLTP.RecordsManagement.UseCases.Reflection;
using YourCompany.OLTP.RecordsManagement.UseCases.Reflection.DI;
using YourCompany.OLTP.StateOwnership;
using YourCompany.OLTP.StateOwnership.Reflection;
using YourCompany.OLTP.StateOwnership.Reflection.DI;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class YourCompanyServiceCollectionExtensions
    {
        public static IServiceCollection AddYourCompanyDomainTypes(
            this IServiceCollection services, RecordTypesMap recordTypesMap)
            => services.AddYourCompanyDomainTypes(recordTypesMap, new UseCaseTypesMap(recordTypesMap?.Keys));

        public static IServiceCollection AddYourCompanyDomainTypes(
            this IServiceCollection services, RecordTypesMap recordTypesMap, UseCaseTypesMap useCaseTypesMap)
        {
            if (recordTypesMap == null) throw new ArgumentNullException(nameof(recordTypesMap));
            if (useCaseTypesMap == null) throw new ArgumentNullException(nameof(useCaseTypesMap));

            services.AddSingleton(recordTypesMap);
            services.AddSingleton(useCaseTypesMap);
            services.AddScoped<ScopedRecordsProvider>();
            services.AddScoped<ScopedUseCasesProvider>();
            services.AddScoped<ScopedRecordsBatchTransactionFactory>();

            services.AddScoped(typeof(IRepository<>), typeof(ScopedRepository<>));
            services.AddSingleton(typeof(IStateAccess<>), typeof(SingletonStateAccess<>));

            foreach (var recordTypeInfo in recordTypesMap.Values)
                if (recordTypeInfo.IsRecord)
                    ScopedRecordsProvider.RegisterRecord(recordTypeInfo, services);

            foreach (var useCaseTypeInfo in useCaseTypesMap.Values)
                if (useCaseTypeInfo.IsUseCasesImplementor)
                    ScopedUseCasesProvider.RegisterImplementedUseCases(useCaseTypeInfo, services);

            return services;
        }

        public static IServiceCollection OverrideYourCompanyScopedRepositoryRecordsDataAccessProvider<TRecord>(
            this IServiceCollection services,
            Func<IServiceProvider, IRecordsDataAccessProvider> recordsDataAccessProviderFactory)
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