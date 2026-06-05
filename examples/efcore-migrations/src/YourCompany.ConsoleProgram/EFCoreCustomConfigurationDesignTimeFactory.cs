using YourCompany.EFCore;
using YourCompany.Configuration.EFCore;

namespace YourCompany.ConsoleProgram
{
    public sealed class EFCoreCustomConfigurationDesignTimeFactory
        : YourCompanyDbContextFactory<CustomConfiguration>.CustomConfigurationDesignTimeForMigrations
    { }
}