using Microsoft.Extensions.Configuration;

namespace YourCompany.Configuration.KeyFileToBase64
{
    internal sealed class KeyFileToBase64ConfigurationSource : FileConfigurationSource
    {
        internal string BasePath { get; init; }

        public override IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            EnsureDefaults(builder);
            return new KeyFileToBase64ConfigurationProvider(this);
        }
    }
}