using YourCompany.Configuration.EFCore;
using YourCompany.Configuration.EFCore.Hosting;

namespace YourCompany.EFCore
{
    public sealed class CustomConfiguration : YourCompanyDbContextConfiguration,
        YourCompanyDbContextConfigurationInterfaces.IMigrationRunner
    {
        public bool MigrateDatabaseOnStart { get; init; }
    }
}