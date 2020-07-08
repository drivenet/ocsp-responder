using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

namespace OcspResponder.Core.Infrastructure
{
    internal sealed class LoggingCaDatabaseUpdater : ICaDatabaseUpdater
    {
        private readonly ICaDatabaseUpdater _inner;
        private readonly ILogger _logger;

        public LoggingCaDatabaseUpdater(ICaDatabaseUpdater inner, ILogger<ICaDatabaseUpdater> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable Update(IReadOnlyCollection<DefaultCaDescription> descriptions)
        {
            _logger.LogInformation(EventIds.UpdatingDatabase, "Updating CA database, descriptions: {Count}", descriptions.Count);
            return _inner.Update(descriptions);
        }

        private static class EventIds
        {
            public static readonly EventId UpdatingDatabase = new EventId(1, nameof(UpdatingDatabase));
        }
    }
}
