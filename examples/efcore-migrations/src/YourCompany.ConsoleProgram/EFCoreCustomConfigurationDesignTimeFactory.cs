using YourCompany.EFCore;
using YourCompany.OLTP.RecordsManagement.Persistence.Linq.EFCore;

namespace YourCompany.ConsoleProgram
{
    public sealed class EFCoreCustomConfigurationDesignTimeFactory
        : YourCompanyDbContextFactory<CustomConfiguration>.CustomConfigurationDesignTimeForMigrations
    { }
}