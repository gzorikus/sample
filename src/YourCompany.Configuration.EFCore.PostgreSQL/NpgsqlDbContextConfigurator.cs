using System;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Npgsql;

using Migrations = Microsoft.EntityFrameworkCore.Migrations;

namespace YourCompany.Configuration.EFCore.PostgreSQL
{
    internal sealed class NpgsqlDbContextConfigurator : YourCompanyDbContextConfigurator
    {
        // Why not just separate directories within the same assembly? Heh, bcs the tool's --output-dir doesn't work as you'd expect!
        // When you deal with generic db context type ending up in several migration tracks (assembly subfolders) the tool
        // fails to find the right *Snapshot.cs file and overwrites the first it finds alphabetically 😢
        // We use the split assembly approach for demo purposes, since in practice it shouldn't be a problem with a single context.
        // But keep in mind there's also an option to extract the assembly name to the config skipped for brevity.
        private const string MigrationAssemblyNameFormatFromConfigurationType
            = $"{EnvironmentConventions.YourCompanyAssemblyPrefix}{nameof(EFCore)}.{nameof(PostgreSQL)}.{{0}}{nameof(Migrations)}";

        private PrefixedObjectNamesShortener _prefixedObjectNamesShortener;

        internal override string SanitizeObjectNamesPrefix(
            YourCompanyDbContextConfiguratorsLoadingContext context, string objectNamesPrefix)
        {
            var shortener = Interlocked.CompareExchange(ref _prefixedObjectNamesShortener, null, null);
            if (shortener == null)
            {
                shortener = new PrefixedObjectNamesShortener(objectNamesPrefix);
                if (Interlocked.CompareExchange(ref _prefixedObjectNamesShortener, shortener, null) != null)
                    throw new ApplicationException("Interlocked.CompareExchange(ref _prefixedObjectNamesShortener, shortener, null) != null");
            }

            if (shortener.ObjectNamesPrefix != objectNamesPrefix) throw new ApplicationException("shortener.ObjectNamesPrefix != objectNamesPrefix");
            return shortener.SanitizedObjectNamesPrefix ?? throw new ApplicationException("shortener.SanitizedObjectNamesPrefix == null");
        }

        internal override string PrefixDbObjectName(
            YourCompanyDbContextConfiguratorsLoadingContext context, string sanitizedObjectNamesPrefix, string objectName)
        {
            var shortener = Interlocked.CompareExchange(ref _prefixedObjectNamesShortener, null, null)
                ?? throw new ApplicationException("Interlocked.CompareExchange(ref _prefixedObjectNamesShortener, null, null) == null");
            if (shortener.SanitizedObjectNamesPrefix != sanitizedObjectNamesPrefix)
                throw new ApplicationException("shortener.SanitizedObjectNamesPrefix != sanitizedObjectNamesPrefix");
            return shortener.PrefixDbObjectName(context, objectName);
        }

        public override void OnConfiguring(
            YourCompanyDbContextConfiguratorsLoadingContext context, DbContextOptionsBuilder optionsBuilder)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (optionsBuilder == null) throw new ArgumentNullException(nameof(optionsBuilder));
            string objectNamesPrefix = context.Configuration.GetYourCompanyInfraObjectNamesPrefix();
            var csb = new NpgsqlConnectionStringBuilder(context.Configuration.GetConnectionString(nameof(Npgsql)));
            optionsBuilder.UseNpgsql(
                connectionString: csb.ConnectionString,
                builder => builder
                    .MigrationsAssembly(string.Format(
                        MigrationAssemblyNameFormatFromConfigurationType, context.DbContextConfigurationType.Name))
                    .MigrationsHistoryTable(
                        tableName: SanitizeObjectNamesPrefix(context, objectNamesPrefix) + HistoryRepository.DefaultTableName,
                        schema: csb.SearchPath)); // setting schema as a workaround (not used from connection string) see https://github.com/npgsql/efcore.pg/issues/2878#issuecomment-2369464947
            optionsBuilder.AddInterceptors(NpgsqlPessimisticLockingUpdateInterceptor.Default);
        }
    }
}