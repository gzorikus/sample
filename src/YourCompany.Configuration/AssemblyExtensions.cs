using System.IO;
using YourCompany.Configuration;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace System.Reflection
#pragma warning restore IDE0130 // Namespace does not match folder structure
{
    public static class AssemblyExtensions
    {
        public static string ResolveYourCompanyAssemblyRelativePath(
            this Assembly assembly, string absoluteOrAssemblyRelativePath)
        {
            if (absoluteOrAssemblyRelativePath == null) throw new ArgumentNullException(nameof(absoluteOrAssemblyRelativePath));
            if (!assembly.CheckIsYourCompanyAssembly()) throw new ApplicationException("!assembly.CheckIsYourCompanyAssembly()");
            return Path.IsPathRooted(absoluteOrAssemblyRelativePath)
                ? absoluteOrAssemblyRelativePath
                : Path.Combine(assembly.GetYourCompanyAssemblyDirectory(), absoluteOrAssemblyRelativePath);
        }

        public static string GetYourCompanyAssemblyDirectory(this Assembly assembly)
        {
            if (!assembly.CheckIsYourCompanyAssembly()) throw new ApplicationException("!assembly.CheckIsYourCompanyAssembly()");
            string location = assembly.Location ?? throw new ApplicationException("assembly.Location == null");
            return Path.GetDirectoryName(location);
        }

        public static bool CheckIsYourCompanyAssembly(this Assembly assembly)
            => !string.IsNullOrEmpty(assembly.FullName)
            && assembly.FullName.StartsWith(EnvironmentConventions.YourCompanyAssemblyPrefix);
    }
}