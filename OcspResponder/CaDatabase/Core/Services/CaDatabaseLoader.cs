using System;
using System.IO;
using System.Linq;

using OcspResponder.CaDatabase.Services;

namespace OcspResponder.CaDatabase.Core.Services
{
    internal sealed class CaDatabaseLoader : IDisposable, ICaDatabaseLoader
    {
        private readonly ICaDatabaseUpdater _updater;
        private readonly CaDescriptionLoader _loader;
        private readonly CaDatabasePathsSource _fileSource;
        private IDisposable? _cleanup;

        public CaDatabaseLoader(ICaDatabaseUpdater updater, CaDescriptionLoader loader, CaDatabasePathsSource fileSource)
        {
            _updater = updater ?? throw new ArgumentNullException(nameof(updater));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _fileSource = fileSource ?? throw new ArgumentNullException(nameof(fileSource));
        }

        public void Dispose()
        {
            _cleanup?.Dispose();
        }

        public void Load()
        {
            if (_cleanup is { } cleanup)
            {
                _cleanup = null;
                cleanup.Dispose();
            }

            var files = _fileSource.GetPaths();
            var count = files.Count;
            if (count == 0)
            {
                throw new InvalidDataException("No CA descriptions were found.");
            }

            var now = DateTimeOffset.UtcNow;
            var descriptions = files
                .Select(file => _loader.Load(file.DbFilePath, file.CertFilePath, now))
                .ToList();
            _cleanup = _updater.Update(descriptions);
        }
    }
}
