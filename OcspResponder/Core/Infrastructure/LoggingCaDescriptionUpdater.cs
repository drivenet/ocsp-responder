using System;
using System.Collections.Generic;

using Microsoft.Extensions.Logging;

namespace OcspResponder.Core.Infrastructure
{
    internal sealed class LoggingCaDescriptionUpdater : ICaDescriptionUpdater
    {
        private readonly ICaDescriptionUpdater _inner;
        private readonly ILogger _logger;

        public LoggingCaDescriptionUpdater(ICaDescriptionUpdater inner, ILogger<ICaDescriptionUpdater> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IDisposable Update(IReadOnlyCollection<DefaultCaDescription> descriptions)
        {
            _logger.LogInformation(EventIds.UpdatingDescriptions, "Updating descriptions, count: {Count}", descriptions.Count);
            return _inner.Update(descriptions);
        }

        private static class EventIds
        {
            public static readonly EventId UpdatingDescriptions = new EventId(1, nameof(UpdatingDescriptions));
        }
    }
}
