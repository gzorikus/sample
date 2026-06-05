#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.Configuration
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class ConfigurationExtensions
    {
        public static IConfigurationSection GetYourCompanyRequiredEFCoreSection(this IConfiguration configuration)
            => configuration.GetYourCompanyRequiredSection().GetRequiredEFCoreSection();

        public static IConfigurationSection GetRequiredEFCoreSection(this IConfigurationSection configuration)
            => configuration.GetRequiredSection(nameof(YourCompany.Configuration.EFCore));
    }
}