using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using YourCompany.OLTP.RecordsManagement.DI;
using YourCompany.OLTP.RecordsManagement.DI.EFCore;
using YourCompany.OLTP.StateOwnership.Reflection;

using TCurrentAssemblyLoadingConfiguration = YourCompany.OLTP.RecordsManagement.DI.EFCore
    .YourCompanyDbContextFactoryLoadingConfiguration;

using TCurrentAssemblyDbContextConfiguration = YourCompany.OLTP.RecordsManagement.DI.EFCore
    .YourCompanyDbContextConfiguration;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.DependencyInjection
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class YourCompanyServiceCollectionExtensions
    {
        public static IServiceCollection AddYourCompanyEFCoreSingleScopedRecordsDataAccessProvider(
            this IServiceCollection services)
        {
            if (services.Any(x => x.ServiceType == typeof(IRecordsDataAccessProvider)))
                throw new ApplicationException("services.Any(x => x.ServiceType == typeof(IRecordsDataAccessProvider))");

            services.AddScoped<IRecordsDataAccessProvider, ScopedRecordsDataAccessProvider>();
            return services.AddYourCompanyEFCoreScopedDbContextFactory();
        }

        public static IServiceCollection AddYourCompanyEFCoreSingleScopedRecordsDataAccessProvider<TConfiguration>(
            this IServiceCollection services)
            where TConfiguration : TCurrentAssemblyDbContextConfiguration, new()
        {
            if (services.Any(x => x.ServiceType == typeof(IRecordsDataAccessProvider)))
                throw new ApplicationException("services.Any(x => x.ServiceType == typeof(IRecordsDataAccessProvider))");

            services.AddScoped<IRecordsDataAccessProvider, ScopedRecordsDataAccessProvider<TConfiguration>>();
            return services.AddYourCompanyEFCoreScopedDbContextFactory<TConfiguration>();
        }

        public static IServiceCollection OverrideYourCompanyEFCoreScopedRepositoryRecordsDataAccessProvider<
            TRecord, TConfiguration>(
            this IServiceCollection services)
            where TRecord : class
            where TConfiguration : TCurrentAssemblyDbContextConfiguration, new()
        {
            return services.OverrideYourCompanyScopedRepositoryRecordsDataAccessProvider<TRecord>(
                provider => new ScopedRecordsDataAccessProvider<TConfiguration>(
                    provider.GetRequiredService<IDbContextFactory<
                        YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore.YourCompanyDbContext<TConfiguration>>>()));
        }

        public static IServiceCollection AddYourCompanyEFCoreScopedDbContextFactory(this IServiceCollection services)
            => services
                .AddScoped<
                    IDbContextFactory<
                        YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore.YourCompanyDbContext<
                            TCurrentAssemblyDbContextConfiguration>>,
                    ScopedYourCompanyDbContextFactory>()
                .AddScoped<
                    IDbContextFactory<
                        YourCompany.Configuration.EFCore.YourCompanyDbContext<
                            TCurrentAssemblyDbContextConfiguration>>,
                    ScopedYourCompanyDbContextFactory>();

        public static IServiceCollection AddYourCompanyEFCoreScopedDbContextFactory<TConfiguration>(
            this IServiceCollection services)
            where TConfiguration : TCurrentAssemblyDbContextConfiguration, new()
            => services
                .AddScoped<
                    IDbContextFactory<
                        YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore.YourCompanyDbContext<TConfiguration>>,
                    ScopedYourCompanyDbContextFactory<TCurrentAssemblyLoadingConfiguration, TConfiguration>>()
                .AddScoped<
                    IDbContextFactory<
                        YourCompany.Configuration.EFCore.YourCompanyDbContext<TConfiguration>>,
                    ScopedYourCompanyDbContextFactory<TCurrentAssemblyLoadingConfiguration, TConfiguration>>();

        public static IServiceCollection AddYourCompanyEFCoreScopedDbContextFactoryLoadedDomainTypes(
            this IServiceCollection services)
            => services.AddYourCompanyDomainTypes(GetLoadedRecordTypesMap());

        public static IServiceCollection AddYourCompanyEFCoreScopedDbContextFactoryLoadedDomainTypes<TConfiguration>(
            this IServiceCollection services)
            where TConfiguration : TCurrentAssemblyDbContextConfiguration, new()
            => services.AddYourCompanyDomainTypes(GetLoadedRecordTypesMap<TConfiguration>());

        public static void UseYourCompanyEFCoreScopedDbContextFactoryLoadedConfiguration(
            this IServiceCollection services)
            => services.AddSingleton(GetLoadedConfiguration());

        public static void UseYourCompanyEFCoreScopedDbContextFactoryLoadedConfiguration<TConfiguration>(
            this IServiceCollection services)
            where TConfiguration : TCurrentAssemblyDbContextConfiguration, new()
            => services.AddSingleton(GetLoadedConfiguration<TConfiguration>());

        public static IConfiguration GetLoadedConfiguration()
        {
            EnsureCurrentAssemblyDbContextConfigurationFactoryLoaded();
            return ScopedYourCompanyDbContextFactory.LoadingContext.Configuration;
        }

        public static IConfiguration GetLoadedConfiguration<TConfiguration>()
            where TConfiguration : TCurrentAssemblyDbContextConfiguration, new()
        {
            EnsureCurrentAssemblyLoadingConfigurationFactoryLoaded<TConfiguration>();
            return ScopedYourCompanyDbContextFactory<TCurrentAssemblyLoadingConfiguration, TConfiguration>
                .LoadingContext
                .Configuration;
        }

        public static RecordTypesMap GetLoadedRecordTypesMap()
        {
            EnsureCurrentAssemblyDbContextConfigurationFactoryLoaded();
            return ScopedYourCompanyDbContextFactory.LoadingContext.RecordTypesMap;
        }

        public static RecordTypesMap GetLoadedRecordTypesMap<TConfiguration>()
            where TConfiguration : TCurrentAssemblyDbContextConfiguration, new()
        {
            EnsureCurrentAssemblyLoadingConfigurationFactoryLoaded<TConfiguration>();
            return ScopedYourCompanyDbContextFactory<TCurrentAssemblyLoadingConfiguration, TConfiguration>
                .LoadingContext
                .RecordTypesMap;
        }

        private static void EnsureCurrentAssemblyDbContextConfigurationFactoryLoaded()
            => ScopedYourCompanyDbContextFactory.EnsureFactoryLoaded();

        private static void EnsureCurrentAssemblyLoadingConfigurationFactoryLoaded<TConfiguration>()
            where TConfiguration : TCurrentAssemblyDbContextConfiguration, new()
            => ScopedYourCompanyDbContextFactory<TCurrentAssemblyLoadingConfiguration, TConfiguration>
                .EnsureFactoryLoaded();
    }
}