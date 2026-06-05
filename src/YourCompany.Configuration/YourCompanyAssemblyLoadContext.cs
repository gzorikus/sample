using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;

namespace YourCompany.Configuration
{
    public sealed class YourCompanyAssemblyLoadContext : AssemblyLoadContext
    {
        private static readonly AssemblyDependencyResolver EntryAssemblyDependencyResolver
            = new(EnvironmentConventions.GetYourCompanyEntryAssembly().Location);

        private readonly List<AssemblyDependencyResolver> _resolvers = new();
        private readonly List<string> _absolutePluginPaths = new();

        public YourCompanyAssemblyLoadContext(string name) : base(name, isCollectible: false) { }

        public void AddPluginPath(string absoluteOrEntryRelativePluginPath)
        {
            var entryAssembly = EnvironmentConventions.GetYourCompanyEntryAssembly();
            string pluginPath = entryAssembly.ResolveYourCompanyAssemblyRelativePath(absoluteOrEntryRelativePluginPath);
            if (TryLoadEntryDependency(pluginPath))
                return;

            var resolver = new AssemblyDependencyResolver(pluginPath);
            _resolvers.Add(resolver);
            _absolutePluginPaths.Add(pluginPath);
        }

        public void LoadAll()
        {
            for (int i = 0; i < _absolutePluginPaths.Count; i++)
                LoadFromAssemblyPath(_absolutePluginPaths[i]);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (EntryAssemblyDependencyResolver.ResolveAssemblyToPath(assemblyName) != null)
                return null;

            for (int i = 0; i < _resolvers.Count; i++)
            {
                var assemblyPath = _resolvers[i].ResolveAssemblyToPath(assemblyName);
                if (assemblyPath != null)
                    return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            if (EntryAssemblyDependencyResolver.ResolveUnmanagedDllToPath(unmanagedDllName) != null)
                return IntPtr.Zero;

            for (int i = 0; i < _resolvers.Count; i++)
            {
                string libraryPath = _resolvers[i].ResolveUnmanagedDllToPath(unmanagedDllName);
                if (libraryPath != null)
                    return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }

        private static bool TryLoadEntryDependency(string pluginPath)
        {
            var potentialEntryDependencyAssemblyName = AssemblyName.GetAssemblyName(pluginPath);
            bool isEntryDependency
                = EntryAssemblyDependencyResolver.ResolveAssemblyToPath(potentialEntryDependencyAssemblyName) != null;
            if (isEntryDependency) Default.LoadFromAssemblyName(potentialEntryDependencyAssemblyName);
            return isEntryDependency;
        }
    }
}