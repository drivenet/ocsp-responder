using System;

namespace OcspResponder.Responder.Core.Services
{
    internal sealed class CaDatabaseLoaderOptions
    {
        private TimeSpan _loadInterval = TimeSpan.FromSeconds(10);

        public TimeSpan LoadInterval
        {
            get => _loadInterval;
            set => _loadInterval = value == TimeSpan.Zero || value.TotalSeconds >= 1
                ? value
                : throw new ArgumentOutOfRangeException(nameof(LoadInterval), value, "Invalid CA database load interval.");
        }
    }
}
