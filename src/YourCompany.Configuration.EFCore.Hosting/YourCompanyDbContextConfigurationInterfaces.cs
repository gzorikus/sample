namespace YourCompany.Configuration.EFCore.Hosting
{
    public static class YourCompanyDbContextConfigurationInterfaces
    {
        public interface IMigrationRunner
        {
            bool MigrateDatabaseOnStart { get; init; }
        }
    }
}