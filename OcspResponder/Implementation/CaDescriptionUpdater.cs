using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Extensions.Logging;

namespace OcspResponder.Implementation
{
    internal sealed class CaDescriptionUpdater : IDisposable
    {
        private readonly CaDescriptionStore _store;
        private readonly CaDescriptionLoader _loader;
        private readonly CaDescriptionFilesSource _fileSource;
        private readonly ILogger _logger;
        private IDisposable? _cleanup;

        public CaDescriptionUpdater(CaDescriptionStore store, CaDescriptionLoader loader, CaDescriptionFilesSource fileSource, ILogger<CaDescriptionUpdater> logger)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _fileSource = fileSource ?? throw new ArgumentNullException(nameof(fileSource));
            _logger = logger;
        }

        public void Dispose()
        {
            _store.Dispose();
            _cleanup?.Dispose();
        }

        public void Update()
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
            _logger.LogInformation(EventIds.UpdatingDescriptions, "Updating descriptions, count: {Count}", count);
            _cleanup = _store.Update(descriptions);
        }

        private static class EventIds
        {
            public static readonly EventId UpdatingDescriptions = new EventId(1, nameof(UpdatingDescriptions));
        }
    }
}
