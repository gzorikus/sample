using System;
using System.Collections.Generic;

namespace YourCompany.Configuration.EFCore.Sqlite
{
    internal sealed partial class SqliteDbContextConfigurator
    {
        // use another or take from config in the loading context depending on your instance and needs
        private const string AssumeUnchangedDefaultCollation = "BINARY";

        internal override IEqualityComparer<string> GetCollationCompatibleStringEqualityComparer(
            YourCompanyDbContextConfiguratorsLoadingContext context, string collationNameOrProviderDefault = null)
        {
            string collationName = collationNameOrProviderDefault ?? AssumeUnchangedDefaultCollation;
            return collationName switch
            {
                AssumeUnchangedDefaultCollation => StringComparer.Ordinal,
                _ => throw new ArgumentOutOfRangeException(nameof(collationNameOrProviderDefault))
            };
        }

        internal override IComparer<string> GetCollationCompatibleStringComparer(
            YourCompanyDbContextConfiguratorsLoadingContext context, string collationNameOrProviderDefault = null)
        {
            string collationName = collationNameOrProviderDefault ?? AssumeUnchangedDefaultCollation;
            return collationName switch
            {
                AssumeUnchangedDefaultCollation => StringComparer.Ordinal,
                _ => throw new ArgumentOutOfRangeException(nameof(collationNameOrProviderDefault))
            };
        }
    }
}