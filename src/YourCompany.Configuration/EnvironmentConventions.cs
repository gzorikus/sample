using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace YourCompany.Configuration
{
    public static class EnvironmentConventions
    {
        public const string DevEnv = "Development";
        public const string YourCompanyAssemblyPrefix = "YourCompany.";
        public const string YourCompanyConfigurationSectionName = nameof(YourCompany);
        internal const string ToBeConfiguredOnlyEnvironmentVariablesPrefix = YourCompanyEnvVarPrefix + "CONFIG_";
        private const string YourCompanyEnvVarPrefix = "YOURCOMPANY_";
        private const string DefaultSecretsSubdirectory = "secrets";

        private static Assembly YourCompanySingleEntryAssembly;

        public static Assembly GetYourCompanyEntryAssembly()
        {
            var singleAssemblyHavingUserSecretsId = Interlocked.CompareExchange(
                            ref YourCompanySingleEntryAssembly, null, null);
            if (singleAssemblyHavingUserSecretsId != null) return singleAssemblyHavingUserSecretsId;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0; i < assemblies.Length; i++)
            {
                var potentialEntryAssembly = assemblies[i];
                if (potentialEntryAssembly.IsDynamic || potentialEntryAssembly.FullName == null) continue;
                if (!potentialEntryAssembly.FullName.StartsWith(YourCompanyAssemblyPrefix)) continue;

                // ⚠️ make sure your application entry project (one per solution) is referencing Microsoft.Extensions.Configuration.UserSecrets ⚠️ 
                if (potentialEntryAssembly.GetCustomAttribute<UserSecretsIdAttribute>() != null)
                {
                    if (singleAssemblyHavingUserSecretsId != null)
                        throw new ApplicationException("singleAssemblyHavingUserSecretsId != null");
                    singleAssemblyHavingUserSecretsId = potentialEntryAssembly;
                }
            }

            if (singleAssemblyHavingUserSecretsId == null)
                throw new ApplicationException("singleAssemblyHavingUserSecretsId == null");

            var alreadyCached = Interlocked.CompareExchange(
                ref YourCompanySingleEntryAssembly, singleAssemblyHavingUserSecretsId, null);

            if (alreadyCached != null && singleAssemblyHavingUserSecretsId != alreadyCached)
                throw new ApplicationException("singleAssemblyHavingUserSecretsId != alreadyCached");
            if (string.IsNullOrEmpty(singleAssemblyHavingUserSecretsId.Location))
                throw new ApplicationException("string.IsNullOrEmpty(singleAssemblyHavingUserSecretsId.Location)");
            if (string.IsNullOrEmpty(singleAssemblyHavingUserSecretsId.FullName))
                throw new ApplicationException("string.IsNullOrEmpty(singleAssemblyHavingUserSecretsId.FullName)");

            return singleAssemblyHavingUserSecretsId;
        }

        public static class ProcessStartEnvVars
        {
            public const string DotnetEnv = "DOTNET_ENVIRONMENT";
            public const string NetcoreEnv = "ASPNETCORE_ENVIRONMENT";
            public const string OverrideEnvironmentName = "YOURCOMPANY_ENVIRONMENT";
            public const string ForceDevEnvSecretFilesWatching = "YOURCOMPANY_FORCE_DEVENV_SECRET_FILES_WATCHING";
            public const string InferredBasePath = "YOURCOMPANY_BASE_PATH";
            public const string OverrideSecretsPath = "YOURCOMPANY_SECRETS_PATH";
            public const string InfraObjectNamesPrefix = "YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX";
            public const string InfraObjectNamesPrefixDevEnvUseCurrentGitBranchFromRepoPath = "YOURCOMPANY_INFRA_OBJECT_NAMES_PREFIX_DEVENV_USE_CURRENT_GIT_BRANCH_FROM_REPO_PATH";

            internal static class MemorizingHelper
            {
                internal static string GetEnvironment(Dictionary<string, string> overrides)
                {
                    string dotnetEnv = GetNullIfEmptyGeneric(MemorizeNotNullValue(
                        DotnetEnv, overrides, resetIgnoredOverride: true));
                    string netcoreEnv = GetNullIfEmptyGeneric(MemorizeNotNullValue(
                        NetcoreEnv, overrides, resetIgnoredOverride: true));
                    string environmentOverride = GetNullIfEmptyGenericAndMemorize(
                        OverrideEnvironmentName, overrides);

                    if (environmentOverride != null) return environmentOverride;
                    if (dotnetEnv != null && netcoreEnv != null && dotnetEnv != netcoreEnv)
                        throw new ApplicationException("dotnetEnv != netcoreEnv");
                    return dotnetEnv ?? netcoreEnv ?? throw new ApplicationException("ensure DOTNET_ENVIRONMENT or ASPNETCORE_ENVIRONMENT is explicitly set");
                }

                internal static void EnsureInfraObjectNamesPrefix(Dictionary<string, string> overrides)
                {
                    string prefix = GetNullIfEmptyGenericAndMemorize(InfraObjectNamesPrefix, overrides);
                    string useCurrentGitBranchFromRepoPath = GetNullIfEmptyPathAndMemorize(
                        InfraObjectNamesPrefixDevEnvUseCurrentGitBranchFromRepoPath, overrides);

                    if (useCurrentGitBranchFromRepoPath != null && GetEnvironment(overrides) != DevEnv)
                        throw new ApplicationException("useCurrentGitBranch && GetEnvironment(overrides) != DevEnv");

                    prefix ??= string.Empty;
                    if (useCurrentGitBranchFromRepoPath != null)
                        prefix += GitHelper.GetCurrentBranch(useCurrentGitBranchFromRepoPath) + '-';
                    overrides[InfraObjectNamesPrefix] = prefix;
                }

                internal static string[] GetSecretFilePaths(Dictionary<string, string> overrides)
                {
                    string secretsPath = GetSecretsPath(overrides);
                    return Directory.Exists(secretsPath)
                        ? Directory.GetFiles(secretsPath, "*.*", SearchOption.AllDirectories)
                        : Array.Empty<string>();
                }

                internal static string GetSecretsPath(Dictionary<string, string> overrides)
                {
                    string secretsPathOverride = GetNullIfEmptyPathAndMemorize(OverrideSecretsPath, overrides);
                    if (secretsPathOverride != null)
                        return MemorizeAbsolutePathValue(OverrideSecretsPath, overrides);

                    string defaultSecretsPath = Path.Combine(
                        GetYourCompanyEntryAssembly().GetYourCompanyAssemblyDirectory(), DefaultSecretsSubdirectory);

                    overrides[OverrideSecretsPath] = defaultSecretsPath;
                    return defaultSecretsPath;
                }

                internal static bool CheckToReloadSecretsOnChange(Dictionary<string, string> overrides)
                {
                    string env = GetEnvironment(overrides);
                    string forceDevEnvVar = MemorizeNotNullValue(
                        ForceDevEnvSecretFilesWatching, overrides, resetIgnoredOverride: false);

                    var _ = bool.TryParse(forceDevEnvVar, out bool forceDevEnv);
                    if (forceDevEnv && env != DevEnv) throw new ApplicationException("forceDevEnv && env != DevEnv");
                    return env != DevEnv || forceDevEnv;
                }

                private static string GetNullIfEmptyGenericAndMemorize(string variable, Dictionary<string, string> overrides)
                {
                    if (variable == null) throw new ArgumentNullException(nameof(variable));
                    if (overrides == null) throw new ArgumentNullException(nameof(overrides));
                    overrides.TryGetValue(variable, out string valueSet);
                    return GetNullIfEmptyGeneric(valueSet)
                        ?? GetNullIfEmptyGeneric(MemorizeNotNullValue(variable, overrides, resetIgnoredOverride: true));
                }

                private static string GetNullIfEmptyPathAndMemorize(string variable, Dictionary<string, string> overrides)
                {
                    if (variable == null) throw new ArgumentNullException(nameof(variable));
                    if (overrides == null) throw new ArgumentNullException(nameof(overrides));
                    overrides.TryGetValue(variable, out string valueSet);
                    return GetNullIfEmptyPath(valueSet)
                        ?? GetNullIfEmptyPath(MemorizeNotNullValue(variable, overrides, resetIgnoredOverride: true));
                }

                private static string MemorizeNotNullValue(
                    string variable, Dictionary<string, string> overrides, bool resetIgnoredOverride)
                {
                    if (variable == null) throw new ArgumentNullException(nameof(variable));
                    if (overrides == null) throw new ArgumentNullException(nameof(overrides));
                    return overrides.TryGetValue(variable, out string envValOverride)
                        && envValOverride != null
                        && !resetIgnoredOverride
                            ? envValOverride
                            : overrides[variable] = Environment.GetEnvironmentVariable(variable) ?? string.Empty;
                }

                private static string MemorizeAbsolutePathValue(string variable, Dictionary<string, string> overrides)
                {
                    if (variable == null) throw new ArgumentNullException(nameof(variable));
                    if (overrides == null) throw new ArgumentNullException(nameof(overrides));
                    string path = overrides[variable] ?? throw new ApplicationException("path == null");
                    string absolutePath = GetYourCompanyEntryAssembly().ResolveYourCompanyAssemblyRelativePath(path);
                    if (path != absolutePath) overrides[variable] = absolutePath;
                    return absolutePath;
                }

                private static string GetNullIfEmptyGeneric(string envVarValue)
                    => string.IsNullOrEmpty(envVarValue) ? null : envVarValue;

                private static string GetNullIfEmptyPath(string envVarValue)
                    => string.IsNullOrWhiteSpace(envVarValue) ? null : envVarValue;
            }
        }
    }
}