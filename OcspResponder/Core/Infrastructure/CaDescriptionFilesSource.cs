using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using static System.FormattableString;

namespace OcspResponder.Core.Infrastructure
{
    internal sealed class CaDescriptionFilesSource
    {
        private static readonly EnumerationOptions EnumerationOptions = new EnumerationOptions { MatchCasing = MatchCasing.CaseSensitive };

        private readonly DirectoryInfo _directory;

        public CaDescriptionFilesSource(string path)
        {
            if (!Path.IsPathFullyQualified(path))
            {
                throw new ArgumentOutOfRangeException(nameof(path), path, "The database file path is not fully qualified.");
            }

            _directory = new DirectoryInfo(path);
        }

        public IReadOnlyList<CaDescriptionFilePaths> GetFiles()
        {
            _directory.Refresh();
            if (!_directory.Exists)
            {
                return Array.Empty<CaDescriptionFilePaths>();
            }

            var dbFiles = _directory.GetFiles("*.db", EnumerationOptions).Select(fileInfo => fileInfo.FullName).ToList();
            var missingFileIds = dbFiles.Select(Path.GetFileNameWithoutExtension).ToHashSet();
            missingFileIds.SymmetricExceptWith(
                _directory.GetFiles("*-ocsp.pfx", EnumerationOptions)
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
                .Select(fileName => new CaDescriptionFilePaths(
                    fileName,
                    Path.Combine(Path.GetDirectoryName(fileName) ?? "", Path.GetFileNameWithoutExtension(fileName) + "-ocsp.pfx")))
                .ToList();
        }
    }
}
