using System;

namespace YourCompany.Configuration.EFCore
{
    public class YourCompanyDbContextConfiguration
    {
        public bool EnableDetailedErrors { get; init; }
        public bool EnableSensitiveDataLogging { get; init; }
        public DateTimeOffset? OverrideYourCompanyDbContextUtcNow { get; init; }
    }
}