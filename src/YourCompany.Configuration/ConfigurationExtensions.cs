using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using YourCompany.Configuration;
using YourCompany.Configuration.KeyFileToBase64;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.Extensions.Configuration
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class ConfigurationExtensions
    {
        private static readonly IReadOnlyDictionary<string, string> NoOverridesButMemorizeProcessStartEnvVars
            = new Dictionary<string, string>();

        public static IConfigurationBuilder AddYourCompanyConfiguration(
            this IConfigurationBuilder builder, IReadOnlyDictionary<string, string> overrides = null)
        {
            var inMemoryOverrides = new Dictionary<string, string>(overrides ?? NoOverridesButMemorizeProcessStartEnvVars);
            if (inMemoryOverrides.ContainsKey(EnvironmentConventions.ProcessStartEnvVars.InferredBasePath))
                throw new ApplicationException("inMemoryOverrides.ContainsKey(EnvironmentConventions.ProcessStartEnvVars.InferredBasePath)");

            var entryAssembly = EnvironmentConventions.GetYourCompanyEntryAssembly();
            string basePath = entryAssembly.GetYourCompanyAssemblyDirectory();
            inMemoryOverrides.Add(EnvironmentConventions.ProcessStartEnvVars.InferredBasePath, basePath);

            string environment = EnvironmentConventions.ProcessStartEnvVars.MemorizingHelper.GetEnvironment(inMemoryOverrides);
            EnvironmentConventions.ProcessStartEnvVars.MemorizingHelper.EnsureInfraObjectNamesPrefix(inMemoryOverrides);
            return builder.SetBasePath(basePath)
                .AddJsonFile(path: "appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile(
                    path: $"appsettings.{environment}.json",
                    optional: true,
                    reloadOnChange: false)
                .AddEnvironmentVariables(EnvironmentConventions.ToBeConfiguredOnlyEnvironmentVariablesPrefix)
                .AddUserSecrets(
                    entryAssembly,
                    optional: true,
                    reloadOnChange: EnvironmentConventions
                        .ProcessStartEnvVars
                        .MemorizingHelper
                        .CheckToReloadSecretsOnChange(inMemoryOverrides))
                .AddRotatingJsonSecrets(inMemoryOverrides)
                .AddInMemoryCollection(inMemoryOverrides);
        }

        public static string GetYourCompanyBasePath(this IConfiguration configuration)
        {
            string basePath = configuration[EnvironmentConventions.ProcessStartEnvVars.InferredBasePath]
                ?? throw new ApplicationException("basePath == null");
            if (!Path.IsPathRooted(basePath)) throw new ApplicationException("!Path.IsPathRooted(basePath)");
            return basePath;
        }

        public static string GetYourCompanyEnvironment(this IConfiguration configuration)
        {
            string environment = configuration[EnvironmentConventions.ProcessStartEnvVars.OverrideEnvironmentName];
            if (string.IsNullOrEmpty(environment)) throw new ApplicationException("string.IsNullOrEmpty(environment)");
            return environment;
        }

        public static string GetYourCompanyInfraObjectNamesPrefix(this IConfiguration configuration)
            => configuration[EnvironmentConventions.ProcessStartEnvVars.InfraObjectNamesPrefix]
                ?? throw new ApplicationException("infraObjectNamesPrefix == null");

        public static IConfigurationSection GetYourCompanyRequiredSection(this IConfiguration configuration)
            => configuration.GetRequiredSection(EnvironmentConventions.YourCompanyConfigurationSectionName);

        private static IConfigurationBuilder AddRotatingJsonSecrets(
            this IConfigurationBuilder builder, Dictionary<string, string> overrides)
        {
            string[] secretFiles = EnvironmentConventions.ProcessStartEnvVars.MemorizingHelper.GetSecretFilePaths(overrides);
            for (int i = 0; i < secretFiles.Length; i++)
            {
                string filePath = secretFiles[i];

                if (filePath.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    builder.AddJsonFile(
                        filePath,
                        optional: true,
                        reloadOnChange: EnvironmentConventions
                            .ProcessStartEnvVars
                            .MemorizingHelper
                            .CheckToReloadSecretsOnChange(overrides));
                }
                else
                {
                    builder.AddKeyFileToBase64(
                        filePath,
                        optional: true,
                        reloadOnChange: EnvironmentConventions
                            .ProcessStartEnvVars
                            .MemorizingHelper
                            .CheckToReloadSecretsOnChange(overrides),
                        overrides);
                }
            }

            return builder;
        }

        private static IConfigurationBuilder AddKeyFileToBase64(
            this IConfigurationBuilder builder,
            string path,
            bool optional,
            bool reloadOnChange,
            Dictionary<string, string> overrides)
        {
            var source = new KeyFileToBase64ConfigurationSource
            {
                Path = path,
                Optional = optional,
                ReloadOnChange = reloadOnChange,
                BasePath = EnvironmentConventions.ProcessStartEnvVars.MemorizingHelper.GetSecretsPath(overrides)
            };

            return builder.Add(source);
        }
    }
}