using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using YourCompany.Configuration.EFCore.CollationAwareSorting;
using YourCompany.Configuration.EFCore.CollationAwareSorting.Metadata;

namespace YourCompany.Configuration.EFCore
{
    public partial class YourCompanyDbContext<TConfiguration> : DbContext
        where TConfiguration : YourCompanyDbContextConfiguration, new()
    {
        private static YourCompanyDbContextConfiguratorsLoadingContext _loadedOnceConfiguratorsLoadingContext;
        private static IReadOnlyList<YourCompanyDbContextConfigurator> _loadedOnceConfigurators;
        private static IModel _singleRuntimeModelPerConfigurationType;
        private static ICollationAwareModelProvider _singleRuntimeCollationAwareSortingModelProviderPerConfigurationType;

        protected static YourCompanyDbContextConfiguratorsLoadingContext ConfiguratorsLoadingContext
            => _loadedOnceConfiguratorsLoadingContext ?? throw new ApplicationException("_loadedOnceConfiguratorsLoadingContext == null");

        protected static IReadOnlyList<YourCompanyDbContextConfigurator> Configurators
            => _loadedOnceConfigurators ?? throw new ApplicationException("_loadedOnceConfigurators == null");

        internal static IModel SingleRuntimeModelPerConfigurationType
            => _singleRuntimeModelPerConfigurationType ?? throw new ApplicationException("_singleRuntimeModelPerConfigurationType == null");

        internal static ICollationAwareModelProvider SingleRuntimeCollationAwareSortingModelProviderPerConfigurationType
            => _singleRuntimeCollationAwareSortingModelProviderPerConfigurationType ?? throw new ApplicationException("_singleRuntimeCollationAwareSortingModelProviderPerConfigurationType == null");

        public TConfiguration Configuration { get; private set; }
        public DateTime ConfiguredUtcNow { get; private set; }

        internal YourCompanyDbContext(
            YourCompanyDbContextConfiguratorsLoadingContext configuratorsLoadingContext,
            IReadOnlyList<YourCompanyDbContextConfigurator> configurators)
        {
            var prevConfiguratorsLoadingContext = Interlocked.CompareExchange(
                ref _loadedOnceConfiguratorsLoadingContext, configuratorsLoadingContext, null);
            if (prevConfiguratorsLoadingContext != null && prevConfiguratorsLoadingContext != configuratorsLoadingContext)
                throw new ApplicationException("prevConfiguratorsLoadingContext != configuratorsLoadingContext");

            var prevConfigurators = Interlocked.CompareExchange(ref _loadedOnceConfigurators, configurators, null);
            if (prevConfigurators != null && prevConfigurators != configurators)
                throw new ApplicationException("prevConfigurators != null && prevConfigurators != configurators");

            if (!configuratorsLoadingContext.IsDesignTime)
            {
                var eagerCreatedModelPerConfigurationType = Model;
                var prevModel = Interlocked.CompareExchange(
                    ref _singleRuntimeModelPerConfigurationType, eagerCreatedModelPerConfigurationType, null);
                if (prevModel != null && prevModel != eagerCreatedModelPerConfigurationType)
                    throw new ApplicationException("prevModel != null && prevModel != eagerCreatedModelPerConfigurationType");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (ConfiguratorsLoadingContext.ModelCreatingAttempt > 0 && !ConfiguratorsLoadingContext.IsDesignTime)
                throw new ApplicationException("ConfiguratorsLoadingContext.ModelCreatingAttempt > 0 && !ConfiguratorsLoadingContext.IsDesignTime");
            ConfiguratorsLoadingContext.ModelCreatingAttempt++;

            for (int i = 0; i < Configurators.Count; i++)
                OnModelCreating(modelBuilder, Configurators[i]);

            OnModelCreatingAfterConfigurators(modelBuilder);
            AfterModelFinalized(modelBuilder.FinalizeModel());
        }

        protected virtual void OnModelCreating(ModelBuilder modelBuilder, YourCompanyDbContextConfigurator configurator)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));
            configurator.OnModelCreating(ConfiguratorsLoadingContext, modelBuilder);
        }

        protected virtual void OnModelCreatingAfterConfigurators(ModelBuilder modelBuilder)
        {
            if (modelBuilder == null) throw new ArgumentNullException(nameof(modelBuilder));
            string objectNamesPrefix = ConfiguratorsLoadingContext.Configuration.GetYourCompanyInfraObjectNamesPrefix();
            if (!string.IsNullOrEmpty(objectNamesPrefix))
                PrefixDbObjects(SanitizeObjectNamesPrefix(objectNamesPrefix), modelBuilder);
        }

        protected virtual void AfterModelFinalized(IModel model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (!ConfiguratorsLoadingContext.IsDesignTime)
            {
                var modelProvider = new CollationAwareSortingSingleModelProvider(
                    model, ConfiguratorsLoadingContext.CollationCompatibleComparersProvider);
                if (Interlocked.CompareExchange(ref _singleRuntimeCollationAwareSortingModelProviderPerConfigurationType, modelProvider, null) != null)
                    throw new ApplicationException("Interlocked.CompareExchange(ref _singleRuntimeCollationAwareSortingModelProviderPerConfigurationType, modelProvider, null) != null");
            }
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (Configuration != null) throw new ApplicationException("Configuration != null");

            for (int i = 0; i < Configurators.Count; i++)
                Configure(optionsBuilder, Configurators[i]);

            Configuration = GetConfigurationSection().Get<TConfiguration>();
            ConfiguredUtcNow = GetConfiguredUtcNow(Configuration);
            ConfigureAfterConfigurators(optionsBuilder, Configuration);

            if (ConfiguredUtcNow.Kind != DateTimeKind.Utc) throw new ApplicationException("ConfiguredUtcNow.Kind != DateTimeKind.Utc");
        }

        protected virtual void Configure(DbContextOptionsBuilder optionsBuilder, YourCompanyDbContextConfigurator configurator)
        {
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));
            if (configurator == null) throw new ArgumentNullException(nameof(configurator));
            configurator.OnConfiguring(ConfiguratorsLoadingContext, optionsBuilder);
        }

        protected virtual IConfigurationSection GetConfigurationSection()
            => ConfiguratorsLoadingContext.Configuration.GetYourCompanyRequiredEFCoreSection();

        protected virtual DateTime GetConfiguredUtcNow(TConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            return config.OverrideYourCompanyDbContextUtcNow?.UtcDateTime ?? DateTime.UtcNow;
        }

        protected virtual void ConfigureAfterConfigurators(DbContextOptionsBuilder optionsBuilder, TConfiguration config)
        {
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));
            if (config == null) throw new ArgumentNullException(nameof(config));
            optionsBuilder.EnableDetailedErrors(config.EnableDetailedErrors);
            optionsBuilder.EnableSensitiveDataLogging(config.EnableSensitiveDataLogging);
        }
    }
}