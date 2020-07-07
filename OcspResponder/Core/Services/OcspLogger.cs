using System;

using Microsoft.Extensions.Logging;

namespace OcspResponder.Core.Services
{
    internal sealed class OcspLogger : IOcspLogger
    {
        private readonly ILogger _logger;

        public OcspLogger(ILogger<OcspResponder> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Debug(string message) => _logger.LogDebug(message);

        public void Error(string message) => _logger.LogError(message);

        public void Warn(string message) => _logger.LogWarning(message);
    }
}
