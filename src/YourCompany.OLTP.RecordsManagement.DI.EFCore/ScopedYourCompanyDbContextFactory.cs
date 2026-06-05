using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using YourCompany.OLTP.RecordsManagement.Persistence.Linq;
using YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore;
using YourCompany.OLTP.StateOwnership.Reflection;

namespace YourCompany.OLTP.RecordsManagement.DI.EFCore
{
    internal sealed class ScopedYourCompanyDbContextFactory
        : ScopedYourCompanyDbContextFactory<YourCompanyDbContextFactoryLoadingConfiguration, YourCompanyDbContextConfiguration>
    {
        public ScopedYourCompanyDbContextFactory(IServiceProvider provider) : base(provider) { }
    }

    internal class ScopedYourCompanyDbContextFactory<TLoadingConfiguration, TDbContextConfiguration> :
        YourCompanyDbContextFactory<TLoadingConfiguration, TDbContextConfiguration>.RunTime
        where TLoadingConfiguration : YourCompanyDbContextFactoryLoadingConfiguration, new()
        where TDbContextConfiguration : YourCompanyDbContextConfiguration, new()
    {
        private static ScopedYourCompanyDbContextFactory<TLoadingConfiguration, TDbContextConfiguration> _loadedOnce;
        private readonly IServiceProvider _provider;

        internal static new YourCompanyDbContextConfiguratorsLoadingContext LoadingContext
            => GetLoadedOnce().GetProtectedLoadingContext();

        internal static new TLoadingConfiguration Configuration => GetLoadedOnce().GetProtectedConfiguration();

        internal static new IReadOnlyList<Configuration.EFCore.YourCompanyDbContextConfigurator> Plugins
            => GetLoadedOnce().GetProtectedPlugins();

        public ScopedYourCompanyDbContextFactory(IServiceProvider provider)
            : base(provider?.GetServices<RecordDataQueryBuilder>() ?? YourCompanyDbContextFactory.NoLoadedQueryBuilders)
            => _provider = provider ?? throw new ArgumentNullException(nameof(provider));

        private ScopedYourCompanyDbContextFactory() : base(YourCompanyDbContextFactory.NoLoadedQueryBuilders) { }

        internal static void EnsureFactoryLoaded()
            => _ = new ScopedYourCompanyDbContextFactory<TLoadingConfiguration, TDbContextConfiguration>();

        protected override void LoadOnce(string[] runtimeArgs = null)
        {
            if (runtimeArgs != null) throw new ApplicationException("runtimeArgs != null");
            if (Interlocked.CompareExchange(ref _loadedOnce, this, null) != null)
                return;

            try
            {
                base.LoadOnce(runtimeArgs);
                if (Configuration == null) throw new ApplicationException("Configuration == null");
                if (Plugins == null) throw new ApplicationException("Plugins == null");
            }
            catch
            {
                Interlocked.CompareExchange(ref _loadedOnce, null, this);
                throw;
            }
        }

        protected override RecordTypesMap LoadOnceRecordTypesMap()
        {
            if (Interlocked.CompareExchange(ref _loadedOnce, null, null) != this)
                throw new ApplicationException("Interlocked.CompareExchange(ref _loadedOnce, null, null) != this");
            return base.LoadOnceRecordTypesMap();
        }

        protected override IEnumerable<RecordDataQueryBuilder> EnumerateRecordDataQueryBuilders(Type recordDataType)
        {
            var queryBuilders = base.EnumerateRecordDataQueryBuilders(recordDataType);
            if (queryBuilders == YourCompanyDbContextFactory.NoLoadedQueryBuilders) queryBuilders = null;

            for (int i = 0; i < Plugins.Count; i++)
            {
                if (Plugins[i] is YourCompanyDbContextConfigurator extendedConfigurator)
                {
                    var configuratorQueryBuilders = extendedConfigurator.EnumerateScopedQueryBuilders(
                        LoadingContext, recordDataType, _provider);

                    if (configuratorQueryBuilders != null
                        && configuratorQueryBuilders != YourCompanyDbContextFactory.NoLoadedQueryBuilders)
                        queryBuilders = queryBuilders == null
                            ? configuratorQueryBuilders
                            : queryBuilders.Concat(configuratorQueryBuilders);
                }
            }

            return queryBuilders ?? YourCompanyDbContextFactory.NoLoadedQueryBuilders;
        }

        protected override YourCompanyDbContext<TDbContextConfiguration> CreateInitializedDbContext()
            => new(LoadingContext, Plugins, RuntimeQueryBuilders);

        protected static ScopedYourCompanyDbContextFactory<TLoadingConfiguration, TDbContextConfiguration> GetLoadedOnce()
            => Interlocked.CompareExchange(ref _loadedOnce, null, null) ?? throw new ApplicationException("_loadedOnce == null");

        protected YourCompanyDbContextConfiguratorsLoadingContext GetProtectedLoadingContext() => base.LoadingContext;
        protected TLoadingConfiguration GetProtectedConfiguration() => base.Configuration;
        protected IReadOnlyList<Configuration.EFCore.YourCompanyDbContextConfigurator> GetProtectedPlugins() => base.Plugins;
    }
}