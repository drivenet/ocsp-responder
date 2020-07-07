using System;

using Microsoft.Extensions.Logging;

namespace OcspResponder.Core.Infrastructure
{
    internal sealed class LoggingCaDescriptionsLoader : ICaDescriptionsLoader
    {
        private readonly ICaDescriptionsLoader _inner;
        private readonly ILogger<ICaDescriptionsLoader> _logger;

        public LoggingCaDescriptionsLoader(ICaDescriptionsLoader inner, ILogger<ICaDescriptionsLoader> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Load()
        {
            try
            {
                _inner.Load();
            }
            catch (Exception exception)
            {
                _logger.LogError(EventIds.LoadFailed, exception, "Failed to load CA description.");
                throw;
            }
        }

        private static class EventIds
        {
            public static readonly EventId LoadFailed = new EventId(1, nameof(LoadFailed));
        }
    }
}
