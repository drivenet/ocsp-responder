using System;
using System.IO;
using System.Linq;

namespace OcspResponder.Core.Infrastructure
{
    internal sealed class CaDescriptionsLoader : IDisposable, ICaDescriptionsLoader
    {
        private readonly ICaDescriptionUpdater _updater;
        private readonly CaDescriptionLoader _loader;
        private readonly CaDescriptionFilesSource _fileSource;
        private IDisposable? _cleanup;

        public CaDescriptionsLoader(ICaDescriptionUpdater updater, CaDescriptionLoader loader, CaDescriptionFilesSource fileSource)
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

            var files = _fileSource.GetFiles();
            var count = files.Count;
            if (count == 0)
            {
                throw new InvalidDataException("No CA descriptions to load were found.");
            }

            var now = DateTimeOffset.UtcNow;
            var descriptions = files
                .Select(file => _loader.Load(file.DbFilePath, file.CertFilePath, now))
                .ToList();
            _cleanup = _updater.Update(descriptions);
        }
    }
}
