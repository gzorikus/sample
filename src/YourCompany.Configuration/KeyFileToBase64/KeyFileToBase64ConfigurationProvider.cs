using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace YourCompany.Configuration.KeyFileToBase64
{
    internal sealed class KeyFileToBase64ConfigurationProvider : FileConfigurationProvider
    {
        private readonly string _basePath;

        internal KeyFileToBase64ConfigurationProvider(KeyFileToBase64ConfigurationSource source) : base(source)
        {
            _basePath = source.BasePath?.TrimEnd(Path.DirectorySeparatorChar) ?? "";
        }

        public override void Load(Stream stream)
        {
            byte[] bytes;

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            Data = new Dictionary<string, string>
            {
                [GenerateConfigurationKey()] = Convert.ToBase64String(bytes)
            };
        }

        private string GenerateConfigurationKey()
        {
            var relativePath = Path.GetRelativePath(_basePath, Source.Path);
            return relativePath
                .Replace(Path.DirectorySeparatorChar, ':')
                .Replace(Path.AltDirectorySeparatorChar, ':');
        }
    }
}