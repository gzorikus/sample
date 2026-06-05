using Microsoft.EntityFrameworkCore;

namespace YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations
{
    public class KickStartDbContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("DataSource=file::memory:");
        }
    }
}