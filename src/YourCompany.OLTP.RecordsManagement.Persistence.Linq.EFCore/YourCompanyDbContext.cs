using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore
{
    public partial class YourCompanyDbContext<TConfiguration> : Configuration.EFCore.YourCompanyDbContext<TConfiguration>
        where TConfiguration : YourCompanyDbContextConfiguration, new()
    {
        protected static new YourCompanyDbContextConfiguratorsLoadingContext ConfiguratorsLoadingContext
            => (YourCompanyDbContextConfiguratorsLoadingContext)
            YourCompany.Configuration.EFCore.YourCompanyDbContext<TConfiguration>.ConfiguratorsLoadingContext;

        protected IRecordDataQueryBuildersProvider RuntimeQueryBuilders { get; }
        protected internal Type CreatedForRecordDataType { get; internal set; }

        internal YourCompanyDbContext(
            YourCompanyDbContextConfiguratorsLoadingContext configuratorsLoadingContext,
            IReadOnlyList<Configuration.EFCore.YourCompanyDbContextConfigurator> configurators,
            IRecordDataQueryBuildersProvider runtimeQueryBuilders)
            : base(configuratorsLoadingContext, configurators)
        {
            RuntimeQueryBuilders = runtimeQueryBuilders;
        }

        public RecordDataQueryBuilder.ForSingleRecordData<TRecordData>
            GetQueryBuilderForSpecification<TRecordData, TQueryableRecordData>(object specification)
            where TRecordData : class
            where TQueryableRecordData : class, TRecordData
        {
            if (specification == null) throw new ArgumentNullException(nameof(specification));
            if (RuntimeQueryBuilders == null) throw new ApplicationException("RuntimeQueryBuilders == null");

            var recordDataType = typeof(TRecordData);
            var queryableRecordDataType = GetQueryableRecordDataType(recordDataType);
            if (queryableRecordDataType != typeof(TQueryableRecordData)) throw new ApplicationException("queryableRecordDataType != typeof(TQueryableRecordData)");

            var builders = RuntimeQueryBuilders.GetQueryBuilders(recordDataType);
            if (builders == null) return null;

            RecordDataQueryBuilder singleHandlingBuilder = null;

            for (int i = 0; i < builders.Count; i++)
            {
                var builder = builders[i];
                if (builder.CheckCanHandleSpecification(specification))
                {
                    if (singleHandlingBuilder != null) throw new ApplicationException("singleHandlingBuilder != null");
                    singleHandlingBuilder = builder;
                }
            }

            return singleHandlingBuilder == null
                ? null
                : singleHandlingBuilder as RecordDataQueryBuilder.ForSingleRecordData<TRecordData>
                    ?? throw new ApplicationException("singleHandlingBuilder is not RecordDataQueryBuilder.ForSingleRecordData<TRecordData>");
        }

        protected override void PrefixDbObjects(string sanitizedObjectNamesPrefix, ModelBuilder modelBuilder)
        {
            CollectQueryableRecordDataTypes(sanitizedObjectNamesPrefix, modelBuilder);
            base.PrefixDbObjects(sanitizedObjectNamesPrefix, modelBuilder);
        }
    }
}