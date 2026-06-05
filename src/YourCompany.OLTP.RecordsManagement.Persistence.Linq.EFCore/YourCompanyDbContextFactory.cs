using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using YourCompany.OLTP.StateOwnership.Reflection;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore
{
    internal sealed class YourCompanyDbContextFactory
        : YourCompanyDbContextFactory<YourCompanyDbContextFactoryLoadingConfiguration, YourCompanyDbContextConfiguration>
    {
        internal static RecordDataQueryBuilder[] NoLoadedQueryBuilders => Array.Empty<RecordDataQueryBuilder>();

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
        Configuration.EFCore.YourCompanyDbContextFactory<TLoadingConfiguration, TDbContextConfiguration>
        where TLoadingConfiguration : YourCompanyDbContextFactoryLoadingConfiguration, new()
        where TDbContextConfiguration : YourCompanyDbContextConfiguration, new()
    {
        protected virtual IRecordDataQueryBuildersProvider RuntimeQueryBuilders { get; }

        protected new YourCompanyDbContextConfiguratorsLoadingContext LoadingContext
            => (YourCompanyDbContextConfiguratorsLoadingContext)base.LoadingContext;

        protected override Configuration.EFCore.YourCompanyDbContextConfiguratorsLoadingContext CreateLoadingContext(
            string[] runtimeArgs) => new YourCompanyDbContextConfiguratorsLoadingContext
        {
            RuntimeArgs = runtimeArgs,
            Configuration = CreateConfiguration()
        };

        protected override void LoadOnce(string[] runtimeArgs = null)
        {
            base.LoadOnce(runtimeArgs);
            LoadingContext.RecordTypesMap = LoadOnceRecordTypesMap();
        }

        protected virtual RecordTypesMap LoadOnceRecordTypesMap()
        {
            if (Plugins == null) throw new ApplicationException("Plugins == null");

            var recordTypes = new List<Type>();

            for (int i = 0; i < Plugins.Count; i++)
            {
                if (Plugins[i] is YourCompanyDbContextConfigurator extendedConfigurator)
                {
                    var configuratorRecordTypes = extendedConfigurator.EnumerateRecordTypes(LoadingContext);
                    if (configuratorRecordTypes != null) recordTypes.AddRange(configuratorRecordTypes);
                }
            }

            return new RecordTypesMap(recordTypes);
        }

        protected virtual IEnumerable<RecordDataQueryBuilder> EnumerateRecordDataQueryBuilders(Type recordDataType)
        {
            if (recordDataType == null) throw new ArgumentNullException(nameof(recordDataType));
            if (Plugins == null) throw new ApplicationException("Plugins == null");

            IEnumerable<RecordDataQueryBuilder> queryBuilders = null;

            for (int i = 0; i < Plugins.Count; i++)
            {
                if (Plugins[i] is YourCompanyDbContextConfigurator extendedConfigurator)
                {
                    var configuratorQueryBuilders = extendedConfigurator.EnumerateQueryBuilders(
                        LoadingContext, recordDataType);

                    if (configuratorQueryBuilders != null
                        && configuratorQueryBuilders != YourCompanyDbContextFactory.NoLoadedQueryBuilders)
                        queryBuilders = queryBuilders == null
                            ? configuratorQueryBuilders
                            : queryBuilders.Concat(configuratorQueryBuilders);
                }
            }

            return queryBuilders ?? YourCompanyDbContextFactory.NoLoadedQueryBuilders;
        }

        protected virtual new YourCompanyDbContext<TDbContextConfiguration> CreateInitializedDbContext()
            => new(LoadingContext, Plugins, RuntimeQueryBuilders);

        internal new class RunTime : YourCompanyDbContextFactory<TLoadingConfiguration, TDbContextConfiguration>,
            IDbContextFactory<YourCompanyDbContext<TDbContextConfiguration>>,
            IDbContextFactory<Configuration.EFCore.YourCompanyDbContext<TDbContextConfiguration>>,
            IRecordDataQueryBuildersProvider
        {
            private readonly IEnumerable<RecordDataQueryBuilder> _queryBuilders;
            private RecordDataQueryBuilder.ByRecordDataType _queryBuildersByRecordDataType;

            protected override IRecordDataQueryBuildersProvider RuntimeQueryBuilders => this;

            internal RunTime(IEnumerable<RecordDataQueryBuilder> queryBuilders)
            {
                _queryBuilders = queryBuilders ?? throw new ArgumentNullException(nameof(queryBuilders));
                LoadOnce();
                // 💩 (see ScopedYourCompanyDbContextFactory) due to DI integration and statical nature of plugins we have to avoid this assertion (LoadingContext == null): if (LoadingContext.IsDesignTime) throw new ApplicationException("LoadingContext.IsDesignTime");
            }

            IReadOnlyList<RecordDataQueryBuilder> IRecordDataQueryBuildersProvider.GetQueryBuilders(Type recordDataType)
            {
                if (recordDataType == null) throw new ArgumentNullException(nameof(recordDataType));

                var queryBuilders = Interlocked.CompareExchange(ref _queryBuildersByRecordDataType, null, null);
                if (queryBuilders == null)
                {
                    queryBuilders = new RecordDataQueryBuilder.ByRecordDataType(_queryBuilders);
                    if (queryBuilders.Count > 0)
                        foreach (var kvp in queryBuilders)
                            ((List<RecordDataQueryBuilder>)kvp.Value)
                                .AddRange(EnumerateRecordDataQueryBuilders(recordDataType: kvp.Key));

                    var justCreatedBefore = Interlocked.CompareExchange(ref _queryBuildersByRecordDataType, queryBuilders, null);
                    queryBuilders = justCreatedBefore ?? queryBuilders;
                }

                if (!queryBuilders.TryGetValue(recordDataType, out var queryBuildersForRecordDataType))
                {
                    var enumerated = EnumerateRecordDataQueryBuilders(recordDataType);
                    queryBuildersForRecordDataType = enumerated == YourCompanyDbContextFactory.NoLoadedQueryBuilders
                        ? new List<RecordDataQueryBuilder>()
                        : new List<RecordDataQueryBuilder>(enumerated);
                    var immutablyModified = new RecordDataQueryBuilder.ByRecordDataType(queryBuilders);
                    immutablyModified.Add(recordDataType, queryBuildersForRecordDataType);
                    Interlocked.CompareExchange(ref _queryBuildersByRecordDataType, immutablyModified, queryBuilders);
                }

                return queryBuildersForRecordDataType;
            }

            YourCompanyDbContext<TDbContextConfiguration>
                IDbContextFactory<YourCompanyDbContext<TDbContextConfiguration>>.CreateDbContext()
                    => CreateInitializedDbContext();

            Configuration.EFCore.YourCompanyDbContext<TDbContextConfiguration>
                IDbContextFactory<Configuration.EFCore.YourCompanyDbContext<TDbContextConfiguration>>.CreateDbContext()
                    => CreateInitializedDbContext();
        }

        internal new class DesignTimeSemiSealed : YourCompanyDbContextFactory<TLoadingConfiguration, TDbContextConfiguration>
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