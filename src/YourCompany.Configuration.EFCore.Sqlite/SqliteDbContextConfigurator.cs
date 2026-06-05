using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

using Migrations = Microsoft.EntityFrameworkCore.Migrations;

namespace YourCompany.Configuration.EFCore.Sqlite
{
    internal sealed partial class SqliteDbContextConfigurator : YourCompanyDbContextConfigurator
    {
        // Why not just separate directories within the same assembly? Heh, bcs the tool's --output-dir doesn't work as you'd expect!
        // When you deal with generic db context type ending up in several migration tracks (assembly subfolders) the tool
        // fails to find the right *Snapshot.cs file and overwrites the first it finds alphabetically 😢
        // We use the split assembly approach for demo purposes, since in practice it shouldn't be a problem with a single context.
        // But keep in mind there's also an option to extract the assembly name to the config skipped for brevity.
        private const string MigrationAssemblyNameFormatFromConfigurationType
            = $"{EnvironmentConventions.YourCompanyAssemblyPrefix}{nameof(EFCore)}.{nameof(Sqlite)}.{{0}}{nameof(Migrations)}";

        private const string MemoryDataSource = ":memory:";

        private static readonly Regex SanitizeAsciiOnlyObjectNamesPrefixRegex = new(
            pattern: @"\W", RegexOptions.Compiled | RegexOptions.ECMAScript);

        public override void OnModelCreating(
            YourCompanyDbContextConfiguratorsLoadingContext context, ModelBuilder modelBuilder)
            => modelBuilder.UseCollation(AssumeUnchangedDefaultCollation);

        internal override string SanitizeObjectNamesPrefix(
            YourCompanyDbContextConfiguratorsLoadingContext context, string objectNamesPrefix)
        {
            if (string.IsNullOrEmpty(objectNamesPrefix)) throw new ApplicationException("string.IsNullOrEmpty(objectNamesPrefix)");
            return SanitizeAsciiOnlyObjectNamesPrefixRegex.Replace(objectNamesPrefix, "_");
        }

        internal override string PrefixDbObjectName(
            YourCompanyDbContextConfiguratorsLoadingContext context, string sanitizedObjectNamesPrefix, string objectName)
        {
            if (objectName == null) throw new ArgumentNullException(nameof(objectName));
            if (string.IsNullOrEmpty(sanitizedObjectNamesPrefix)) throw new ApplicationException("string.IsNullOrEmpty(sanitizedObjectNamesPrefix)");
            if (objectName.Contains(sanitizedObjectNamesPrefix)) throw new ApplicationException("objectName.Contains(sanitizedObjectNamesPrefix)");
            return sanitizedObjectNamesPrefix + objectName;
        }

        public override void OnConfiguring(
            YourCompanyDbContextConfiguratorsLoadingContext context, DbContextOptionsBuilder optionsBuilder)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));
            string objectNamesPrefix = context.Configuration.GetYourCompanyInfraObjectNamesPrefix();
            optionsBuilder.UseSqlite(
                connectionString: WithDeterministicPath(context.Configuration.GetConnectionString(nameof(Sqlite))),
                builder => builder
                    .MigrationsAssembly(string.Format(
                        MigrationAssemblyNameFormatFromConfigurationType, context.DbContextConfigurationType.Name))
                    .MigrationsHistoryTable(
                        tableName: SanitizeObjectNamesPrefix(context, objectNamesPrefix) + HistoryRepository.DefaultTableName));
            optionsBuilder.AddInterceptors(SqlitePessimisticLockingUpdateInterceptor.Default);
        }

        private static string WithDeterministicPath(string connectionString)
        {
            if (connectionString == null) throw new ArgumentNullException(nameof(connectionString));
            var builder = new SqliteConnectionStringBuilder(connectionString);
            builder.DataSource = builder.DataSource != MemoryDataSource
                ? EnvironmentConventions
                    .GetYourCompanyEntryAssembly()
                    .ResolveYourCompanyAssemblyRelativePath(builder.DataSource)
                : builder.DataSource;
            return builder.ConnectionString;
        }
    }
}