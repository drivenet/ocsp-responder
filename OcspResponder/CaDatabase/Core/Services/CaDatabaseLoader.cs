using System;
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

            var paths = _fileSource.GetPaths();
            var now = DateTimeOffset.UtcNow;
            var descriptions = paths
                .Select(path => _loader.Load(path, now))
                .ToList();
            _cleanup = _updater.Update(descriptions);
        }
    }
}
