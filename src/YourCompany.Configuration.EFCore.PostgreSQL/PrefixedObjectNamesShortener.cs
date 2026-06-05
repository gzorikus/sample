using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace YourCompany.Configuration.EFCore.PostgreSQL
{
    internal class PrefixedObjectNamesShortener
    {
        private const int NAMEDATALEN = 64;
        private const int MaxIdentifierLengthBytes = NAMEDATALEN - 1; // before PostgreSQL 17 and 127 for 17+
        private const int AssumeMaxObjectNameAsciiPrefixLength = 13;
        private const int AssumeLongObjectNameShaSuffixLength = 7; // almost just as git's sha shortening

        // limiting to ascii identifiers only for simplicity
        private static readonly Encoding AsciiOnlyIdentifiersEncoding = Encoding.GetEncoding(
            "us-ascii",
            new EncoderExceptionFallback(),
            new DecoderExceptionFallback()
        );

        private static readonly Regex SanitizeAsciiOnlyObjectNamesPrefixRegex = new(
            pattern: @"\W", RegexOptions.Compiled | RegexOptions.ECMAScript);

        private readonly HashSet<string> _rejectDuplicateObjectsAndConcurrentPrefixing;
        private int _lastModelCreatingAttempt;

        public string ObjectNamesPrefix { get; }
        public string SanitizedObjectNamesPrefix { get; }

        internal PrefixedObjectNamesShortener(string objectNamesPrefix)
        {
            if (string.IsNullOrEmpty(objectNamesPrefix)) throw new ApplicationException("string.IsNullOrEmpty(objectNamesPrefix)");

            string sanitizedPrefix = SanitizeAsciiOnlyObjectNamesPrefixRegex.Replace(objectNamesPrefix, "_");

            bool leadingUnderscore = sanitizedPrefix.StartsWith('_');
            bool trailingUnderscore = sanitizedPrefix.EndsWith('_');
            sanitizedPrefix = sanitizedPrefix.Trim('_');

            int charactersToTruncate = sanitizedPrefix.Length - AssumeMaxObjectNameAsciiPrefixLength;
            while (charactersToTruncate > 0)
            {
                string shortened = ShortenSanitizedAsciiPrefix(sanitizedPrefix);
                if (sanitizedPrefix.Length <= shortened.Length) throw new ApplicationException("sanitizedPrefix.Length <= shortened.Length");
                charactersToTruncate -= sanitizedPrefix.Length - shortened.Length;
                sanitizedPrefix = shortened;
            }

            if (leadingUnderscore) sanitizedPrefix = '_' + sanitizedPrefix;
            if (trailingUnderscore) sanitizedPrefix += '_';
            
            ObjectNamesPrefix = objectNamesPrefix;
            SanitizedObjectNamesPrefix = sanitizedPrefix;

            _rejectDuplicateObjectsAndConcurrentPrefixing = new HashSet<string>();
        }

        internal string PrefixDbObjectName(YourCompanyDbContextConfiguratorsLoadingContext context, string objectName)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (objectName == null) throw new ArgumentNullException(nameof(objectName));

            if (objectName.Contains(SanitizedObjectNamesPrefix)) throw new ApplicationException("objectName.Contains(SanitizedObjectNamesPrefix)");

            if (SanitizedObjectNamesPrefix.Length + AsciiOnlyIdentifiersEncoding.GetByteCount(objectName) > MaxIdentifierLengthBytes)
            {
                var hashBytes = SHA256.HashData(AsciiOnlyIdentifiersEncoding.GetBytes(objectName));
                string hashString = Convert.ToHexString(hashBytes);
                objectName = objectName[..(MaxIdentifierLengthBytes - SanitizedObjectNamesPrefix.Length - AssumeLongObjectNameShaSuffixLength)];
                objectName += hashString[..AssumeLongObjectNameShaSuffixLength];
            }

            string prefixed = SanitizedObjectNamesPrefix + objectName;

            if (!Monitor.TryEnter(_rejectDuplicateObjectsAndConcurrentPrefixing)) throw new ApplicationException("!Monitor.TryEnter(_rejectDuplicateObjectsAndConcurrentPrefixing)");
            try
            {
                if (_lastModelCreatingAttempt != context.ModelCreatingAttempt) _rejectDuplicateObjectsAndConcurrentPrefixing.Clear();
                if (!_rejectDuplicateObjectsAndConcurrentPrefixing.Add(prefixed)) throw new ApplicationException("!_rejectDuplicateObjectsAndConcurrentPrefixing.Add(prefixed)");
                _lastModelCreatingAttempt = context.ModelCreatingAttempt;
            }
            finally
            {
                Monitor.Exit(_rejectDuplicateObjectsAndConcurrentPrefixing);
            }

            return prefixed;
        }

        private static string ShortenSanitizedAsciiPrefix(string sanitizedPrefix)
        {
            if (sanitizedPrefix == null) throw new ArgumentNullException(nameof(sanitizedPrefix));

            var parts = sanitizedPrefix.Split('_');
            bool anyPartShortened = false;

            for (int i = 0; i < parts.Length; i++)
            {
                string asciiPartBtwUnderscores = parts[i];
                if (asciiPartBtwUnderscores.Length == 0) throw new ApplicationException("asciiPartBtwUnderscores.Length == 0");
                if (asciiPartBtwUnderscores.Length > 1)
                {
                    anyPartShortened = true;
                    parts[i] = asciiPartBtwUnderscores[..^1];
                }
            }

            if (!anyPartShortened && parts.Length < 2) throw new ApplicationException("!anyPartShortening && parts.Length < 2");
            return anyPartShortened ? string.Join('_', parts) : string.Concat(parts);
        }
    }
}