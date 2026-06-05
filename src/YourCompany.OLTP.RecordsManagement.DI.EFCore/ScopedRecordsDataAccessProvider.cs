using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YourCompany.OLTP.RecordsManagement.Persistence;
using YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore;

namespace YourCompany.OLTP.RecordsManagement.DI.EFCore
{
    internal sealed class ScopedRecordsDataAccessProvider
        : ScopedRecordsDataAccessProvider<YourCompanyDbContextConfiguration>
    {
        public ScopedRecordsDataAccessProvider(
            IDbContextFactory<YourCompanyDbContext<YourCompanyDbContextConfiguration>> dbContextFactory)
            : base(dbContextFactory) { }
    }

    internal class ScopedRecordsDataAccessProvider<TConfiguration> : IRecordsDataAccessProvider
        where TConfiguration : YourCompanyDbContextConfiguration, new()
    {
        private static readonly AsyncLocal<YourCompanyDbContext<TConfiguration>> RootDbContext = new();

        private readonly IDbContextFactory<YourCompanyDbContext<TConfiguration>> _dbContextFactory;

        public ScopedRecordsDataAccessProvider(
            IDbContextFactory<YourCompanyDbContext<TConfiguration>> dbContextFactory)
            => _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));

        public async Task<RecordsDataAccess.IStarting> Connect(Type recordDataType, CancellationToken cancellationToken)
        {
            if (recordDataType == null) throw new ArgumentNullException(nameof(recordDataType));

            var contextBeforeConnect = RootDbContext.Value;
            if (contextBeforeConnect == null)
            {
                RootDbContext.Value = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
                if (RootDbContext.Value.CreatedForRecordDataType != null) throw new ApplicationException("RootDbContext.Value.CreatedForRecordDataType != null");
                RootDbContext.Value.CreatedForRecordDataType = recordDataType;
            }

            try
            {
                return new YourCompanyDbContextRecordsDataAccessAdapter<TConfiguration>(
                    recordDataType, RootDbContext.Value, contextCreator: contextBeforeConnect == null);
            }
            catch
            {
                if (contextBeforeConnect == null && RootDbContext.Value != null) await RootDbContext.Value.DisposeAsync();
                RootDbContext.Value = contextBeforeConnect;
                throw;
            }
        }
    }
}