using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Options;

using static System.FormattableString;

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

            var dbFiles = directory.GetFiles("*.db", EnumerationOptions).Select(fileInfo => fileInfo.FullName).ToList();
            var missingFileIds = dbFiles.Select(Path.GetFileNameWithoutExtension).ToHashSet();
            missingFileIds.SymmetricExceptWith(
                directory.GetFiles("*-ocsp.pfx", EnumerationOptions)
                    .Select(fileInfo =>
                    {
                        var name = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                        name = name.Substring(0, name.Length - 5);
                        return name;
                    }));

            if (missingFileIds.Count != 0)
            {
                var ids = string.Join("\", \"", missingFileIds.Take(5));
                throw new InvalidDataException(Invariant($"Some CA description files are missing.\nNon-exhaustive id list: \"{ids}\""));
            }

            return dbFiles
                .Select(fileName => new CaDescriptionPaths(
                    fileName,
                    Path.Combine(Path.GetDirectoryName(fileName) ?? "", Path.GetFileNameWithoutExtension(fileName) + "-ocsp.pfx")))
                .ToList();
        }
    }
}
