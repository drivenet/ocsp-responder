using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Extensions.Options;

namespace OcspResponder.CaDatabase.Core.Services
{
    internal sealed class CaDatabasePathsSource
    {
        private static readonly EnumerationOptions EnumerationOptions = new EnumerationOptions { MatchCasing = MatchCasing.CaseSensitive };

        private readonly IOptionsMonitor<CaDatabaseOptions> _options;

        public CaDatabasePathsSource(IOptionsMonitor<CaDatabaseOptions> options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public IReadOnlyList<CaDescriptionPaths> GetPaths()
        {
            var directory = new DirectoryInfo(_options.CurrentValue.DatabasePath);
            if (!directory.Exists)
            {
                return Array.Empty<CaDescriptionPaths>();
            }

            var paths = new List<CaDescriptionPaths>();
            foreach (var fileInfo in directory.GetFiles("*.db", EnumerationOptions))
            {
                var dbFilePath = fileInfo.FullName;
                var certFilePath = Path.Combine(Path.GetDirectoryName(dbFilePath) ?? "", Path.GetFileNameWithoutExtension(dbFilePath) + "-ocsp.pfx");
                if (!File.Exists(certFilePath))
                {
                    continue;
                }

                paths.Add(new CaDescriptionPaths(dbFilePath, certFilePath));
            }

            return paths;
        }
    }
}
