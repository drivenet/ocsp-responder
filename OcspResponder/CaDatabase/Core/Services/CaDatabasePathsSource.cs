using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OcspResponder.CaDatabase.Core.Services
{
    internal sealed class CaDatabasePathsSource
    {
        private static readonly EnumerationOptions EnumerationOptions = new EnumerationOptions
        {
            MatchCasing = MatchCasing.CaseSensitive,
            RecurseSubdirectories = true,
        };

        private readonly IOptionsMonitor<CaDatabaseOptions> _options;
        private readonly ILogger _logger;

        public CaDatabasePathsSource(IOptionsMonitor<CaDatabaseOptions> options, ILogger<CaDatabasePathsSource> logger)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IReadOnlyList<CaDescriptionPaths> GetPaths()
        {
            var directory = new DirectoryInfo(_options.CurrentValue.DatabasePath);
            if (!directory.Exists)
            {
                return Array.Empty<CaDescriptionPaths>();
            }

            var dbFileInfos = directory.EnumerateFiles("*.db", EnumerationOptions);
            Dictionary<string, string> dbFileMap;
            try
            {
                dbFileMap = dbFileInfos.ToDictionary(
                    fileInfo => fileInfo.Name.Substring(0, fileInfo.Name.Length - 3),
                    fileInfo => fileInfo.FullName);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidDataException("Non-unique database file.", exception);
            }

            var certFileInfos = directory.EnumerateFiles("*-ocsp.pfx", EnumerationOptions);
            Dictionary<string, string> certFileMap;
            try
            {
                certFileMap = certFileInfos.ToDictionary(
                    fileInfo => fileInfo.Name.Substring(0, fileInfo.Name.Length - 9),
                    fileInfo => fileInfo.FullName);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidDataException("Non-unique certificate file.", exception);
            }

            var paths = new List<CaDescriptionPaths>(Math.Min(dbFileMap.Count, certFileMap.Count));
            foreach (var (name, dbFilePath) in dbFileMap)
            {
                if (!certFileMap.Remove(name, out var certFilePath))
                {
                    _logger.LogError(EventIds.MissingCertificate, "Missing certificate for database \"{Name}\"", name);
                    continue;
                }

                paths.Add(new CaDescriptionPaths(dbFilePath, certFilePath));
            }

            foreach (var name in certFileMap.Keys)
            {
                _logger.LogError(EventIds.MissingDatabase, "Missing database for certificate \"{Name}\"", name);
            }

            return paths;
        }

        private static class EventIds
        {
            public static readonly EventId MissingCertificate = new EventId(1, nameof(MissingCertificate));
            public static readonly EventId MissingDatabase = new EventId(2, nameof(MissingDatabase));
        }
    }
}
