using System;

using Microsoft.Extensions.Logging;

using OcspResponder.CaDatabase.Services;

namespace OcspResponder.CaDatabase.Core.Services
{
    internal sealed class LoggingCaDescriptionsLoader : ICaDatabaseLoader
    {
        private readonly ICaDatabaseLoader _inner;
        private readonly ILogger<ICaDatabaseLoader> _logger;

        public LoggingCaDescriptionsLoader(ICaDatabaseLoader inner, ILogger<ICaDatabaseLoader> logger)
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
