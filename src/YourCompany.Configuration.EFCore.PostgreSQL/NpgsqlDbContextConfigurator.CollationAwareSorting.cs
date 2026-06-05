using System;
using System.Collections.Generic;

namespace YourCompany.Configuration.EFCore.PostgreSQL
{
    internal sealed partial class NpgsqlDbContextConfigurator
    {
        // use another or take from config in the loading context depending on your instance and needs
        private const string AssumeOSConsistentLibcBinaryDefaultCollation = "C";

        internal override IEqualityComparer<string> GetCollationCompatibleStringEqualityComparer(
            YourCompanyDbContextConfiguratorsLoadingContext context, string collationNameOrProviderDefault = null)
        {
            string collationName = collationNameOrProviderDefault ?? AssumeOSConsistentLibcBinaryDefaultCollation;
            return collationName switch
            {
                AssumeOSConsistentLibcBinaryDefaultCollation => StringComparer.Ordinal,
                _ => throw new ArgumentOutOfRangeException(nameof(collationNameOrProviderDefault))
            };
        }

        internal override IComparer<string> GetCollationCompatibleStringComparer(
            YourCompanyDbContextConfiguratorsLoadingContext context, string collationNameOrProviderDefault = null)
        {
            string collationName = collationNameOrProviderDefault ?? AssumeOSConsistentLibcBinaryDefaultCollation;
            return collationName switch
            {
                AssumeOSConsistentLibcBinaryDefaultCollation => StringComparer.Ordinal,
                _ => throw new ArgumentOutOfRangeException(nameof(collationNameOrProviderDefault))
            };
        }
    }
}