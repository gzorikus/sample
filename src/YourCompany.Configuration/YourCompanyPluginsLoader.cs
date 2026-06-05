using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Loader;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace YourCompany.Configuration
{
    public abstract class YourCompanyPluginsLoader<TPlugin, TLoadingContext, TLoadingConfiguration>
        where TPlugin : class
        where TLoadingContext : YourCompanyPluginsLoadingContext, new()
        where TLoadingConfiguration : YourCompanyPluginsLoadingConfiguration, new()
    {
        private TLoadingContext _loadingContext;
        private TLoadingConfiguration _configuration;
        private IReadOnlyList<TPlugin> _plugins;
        private Dictionary<string, string> _configurationOverrides;

        protected TLoadingContext LoadingContext => _loadingContext ?? throw new ApplicationException("_loadingContext == null");
        protected TLoadingConfiguration Configuration => _configuration ?? throw new ApplicationException("_configuration == null");
        protected IReadOnlyList<TPlugin> Plugins => _plugins ?? throw new ApplicationException("_plugins == null");

        protected virtual void LoadOnce(string[] runtimeArgs = null)
        {
            if (runtimeArgs != null) CollectYourCompanyConfigurationOverrides(new ReadOnlySpan<string>(runtimeArgs));
            LoadOnce(CreateLoadingContext(runtimeArgs));
        }

        protected virtual void CollectYourCompanyConfigurationOverrides(ReadOnlySpan<string> runtimeArgs)
        {
            while (runtimeArgs.Length > 0)
            {
                int consumedArgs = ConsumeArgsAndCollectYourCompanyConfigurationOverrides(runtimeArgs);
                runtimeArgs = runtimeArgs[consumedArgs..];
            }
        }

        protected virtual int ConsumeArgsAndCollectYourCompanyConfigurationOverrides(ReadOnlySpan<string> runtimeArgs)
        {
            switch (runtimeArgs[0])
            {
                case "--environment":
                    if (runtimeArgs.Length < 2) throw new ArgumentException("runtimeArgs.Length < 2");
                    AddYourCompanyConfigurationOverrideBeforeLoading(
                        EnvironmentConventions.ProcessStartEnvVars.OverrideEnvironmentName, runtimeArgs[1]);
                    return 2;
                case "--override":
                    if (runtimeArgs.Length < 2) throw new ArgumentException("runtimeArgs.Length < 2");
                    var keyValue = runtimeArgs[1].Split('=', 2);
                    if (keyValue.Length < 2) throw new ApplicationException("keyValue.Length < 2");
                    AddYourCompanyConfigurationOverrideBeforeLoading(keyValue[0], keyValue[1]);
                    return 2;
            }

            return 1;
        }

        protected void AddYourCompanyConfigurationOverrideBeforeLoading(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ApplicationException("string.IsNullOrEmpty(key)");
            _configurationOverrides ??= new Dictionary<string, string>();
            _configurationOverrides.Add(key, value ?? string.Empty);
        }

        protected virtual TLoadingContext CreateLoadingContext(string[] runtimeArgs) => new()
        {
            RuntimeArgs = runtimeArgs,
            Configuration = CreateConfiguration()
        };

        protected IConfiguration CreateConfiguration()
            => new ConfigurationBuilder().AddYourCompanyConfiguration(_configurationOverrides).Build();

        protected virtual void LoadOnce(TLoadingContext loadingContext)
        {
            if (loadingContext?.Configuration == null) throw new ApplicationException("loadingContext?.Configuration == null");
            if (Interlocked.CompareExchange(ref _loadingContext, loadingContext, null) != null)
                 throw new ApplicationException("Interlocked.CompareExchange(_loadingContext, loadingContext, null) != null");

            _configuration = GetLoadingConfigurationSection(loadingContext.Configuration.GetYourCompanyRequiredSection())
                .Get<TLoadingConfiguration>();
            var loadContext = LoadPluginAssemblies();
            _plugins = CreatePlugins(loadContext);
        }

        protected virtual IConfigurationSection GetLoadingConfigurationSection(IConfigurationSection fromYourCompanySection)
            => fromYourCompanySection;

        protected virtual TPlugin CreateInstance(Type type) => (TPlugin)Activator.CreateInstance(type);

        private YourCompanyAssemblyLoadContext LoadPluginAssemblies()
        {
            if (_configuration == null) throw new ApplicationException("_configuration == null");
            var loadContext = new YourCompanyAssemblyLoadContext(GetType().Name);
            if (_configuration.PluginPaths == null || _configuration.PluginPaths.Count == 0) return loadContext;

            for (int i = 0; i < _configuration.PluginPaths.Count; i++)
                loadContext.AddPluginPath(_configuration.PluginPaths[i]);

            loadContext.LoadAll();
            return loadContext;
        }

        private IReadOnlyList<TPlugin> CreatePlugins(YourCompanyAssemblyLoadContext loadContext)
        {
            if (_configuration == null) throw new ApplicationException("_configuration == null");
            var plugins = new List<TPlugin>(_configuration.PluginPaths?.Count ?? 0);

            var pluginNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var loadedAssemblies = AssemblyLoadContext.Default.Assemblies.Concat(loadContext.Assemblies);
            foreach (var type in loadedAssemblies.SelectMany(assembly => assembly.GetTypes()))
            {
                if (type.IsAbstract || !typeof(TPlugin).IsAssignableFrom(type) || type == typeof(TPlugin)) continue;
                if (!pluginNames.Add(type.Name)) throw new ApplicationException("!pluginNames.Add(type.Name)");
                plugins.Add(CreateInstance(type));
            }

            return plugins;
        }
    }
}