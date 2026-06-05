using Microsoft.EntityFrameworkCore;

namespace YourCompany.Configuration.EFCore
{
    public abstract class YourCompanyDbContextConfigurator
    {
        public virtual void OnModelCreating(
            YourCompanyDbContextConfiguratorsLoadingContext context, ModelBuilder modelBuilder)
        { }

        internal virtual string SanitizeObjectNamesPrefix(
            YourCompanyDbContextConfiguratorsLoadingContext context, string objectNamesPrefix) => null;

        internal virtual string PrefixDbObjectName(
            YourCompanyDbContextConfiguratorsLoadingContext context, string sanitizedObjectNamesPrefix, string objectName)
            => null;

        public virtual void OnConfiguring(
            YourCompanyDbContextConfiguratorsLoadingContext context, DbContextOptionsBuilder optionsBuilder)
        { }
    }
}