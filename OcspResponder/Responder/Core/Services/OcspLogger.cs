using System;

using Microsoft.Extensions.Logging;

using OcspResponder.Core;

namespace OcspResponder.Responder.Core.Services
{
    internal sealed class OcspLogger : IOcspLogger
    {
        private readonly ILogger _logger;

        public OcspLogger(ILogger<OcspResponder.Core.OcspResponder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Debug(string message) => _logger.LogDebug("{Message}", message);

        public void Error(string message) => _logger.LogError("{Message}", message);

        public void Warn(string message) => _logger.LogWarning("{Message}", message);
    }
}
