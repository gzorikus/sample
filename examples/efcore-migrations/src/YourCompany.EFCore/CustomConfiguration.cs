using YourCompany.Configuration.EFCore.Hosting;
using YourCompany.OLTP.RecordsManagement.DI.EFCore;

namespace YourCompany.EFCore
{
    public sealed class CustomConfiguration : YourCompanyDbContextConfiguration,
        YourCompanyDbContextConfigurationInterfaces.IMigrationRunner
    {
        public bool MigrateDatabaseOnStart { get; init; }
    }
}