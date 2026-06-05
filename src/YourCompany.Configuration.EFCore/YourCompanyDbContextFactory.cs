using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using YourCompany.Configuration.EFCore.CollationAwareSorting;

namespace YourCompany.Configuration.EFCore
{
    internal sealed class YourCompanyDbContextFactory
        : YourCompanyDbContextFactory<YourCompanyDbContextFactoryLoadingConfiguration, YourCompanyDbContextConfiguration>
    {
        internal sealed class DefaultDesignTimeForMigrations
            : YourCompanyDbContextFactory<YourCompanyDbContextConfiguration>.CustomConfigurationDesignTimeForMigrations
        { }
    }

    public static class YourCompanyDbContextFactory<TDbContextConfiguration>
        where TDbContextConfiguration : YourCompanyDbContextConfiguration, new()
    {
        public abstract class CustomConfigurationDesignTimeForMigrations
            : IDesignTimeDbContextFactory<YourCompanyDbContext<TDbContextConfiguration>>
        {
            YourCompanyDbContext<TDbContextConfiguration>
                IDesignTimeDbContextFactory<YourCompanyDbContext<TDbContextConfiguration>>.CreateDbContext(string[] args)
                    => new YourCompanyDbContextFactory<
                        LoadingConfigurationPerDbContextConfigurationForSeparateLoading, TDbContextConfiguration>
                        .DesignTimeSemiSealed()
                        .CreateDbContext(args);
        }

        private sealed class LoadingConfigurationPerDbContextConfigurationForSeparateLoading
            : YourCompanyDbContextFactoryLoadingConfiguration
        {
        }
    }

    internal abstract class YourCompanyDbContextFactory<TLoadingConfiguration, TDbContextConfiguration> :
        YourCompanyPluginsLoader<
            YourCompanyDbContextConfigurator,
            YourCompanyDbContextConfiguratorsLoadingContext,
            TLoadingConfiguration>
        where TLoadingConfiguration : YourCompanyDbContextFactoryLoadingConfiguration, new()
        where TDbContextConfiguration : YourCompanyDbContextConfiguration, new()
    {
        protected override void LoadOnce(YourCompanyDbContextConfiguratorsLoadingContext loadingContext)
        {
            base.LoadOnce(loadingContext);
            loadingContext.LoadingConfigurationType = typeof(TLoadingConfiguration);
            loadingContext.DbContextConfigurationType = typeof(TDbContextConfiguration);
            loadingContext.CollationCompatibleComparersProvider = Plugins.ToCollationCompatibleComparersProvider(loadingContext);
        }

        protected override IConfigurationSection GetLoadingConfigurationSection(IConfigurationSection fromYourCompanySection)
            => fromYourCompanySection.GetRequiredEFCoreSection();

        protected virtual YourCompanyDbContext<TDbContextConfiguration> CreateInitializedDbContext()
            => new(LoadingContext, Plugins);

        internal class RunTime : YourCompanyDbContextFactory<TLoadingConfiguration, TDbContextConfiguration>,
            IDbContextFactory<YourCompanyDbContext<TDbContextConfiguration>>
        {
            internal RunTime()
            {
                LoadOnce();
                // 💩 (see ScopedYourCompanyDbContextFactory) due to DI integration and statical nature of plugins we have to avoid this assertion (LoadingContext == null): if (LoadingContext.IsDesignTime) throw new ApplicationException("LoadingContext.IsDesignTime");
            }

            YourCompanyDbContext<TDbContextConfiguration>
                IDbContextFactory<YourCompanyDbContext<TDbContextConfiguration>>.CreateDbContext()
                    => CreateInitializedDbContext();
        }

        internal class DesignTimeSemiSealed : YourCompanyDbContextFactory<TLoadingConfiguration, TDbContextConfiguration>
        {
            internal YourCompanyDbContext<TDbContextConfiguration> CreateDbContext(string[] args)
            {
                LoadOnce(args);
                if (!LoadingContext.IsDesignTime) throw new ApplicationException("!LoadingContext.IsDesignTime");
                return CreateInitializedDbContext();
            }
        }
    }
}