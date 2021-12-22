using System;

namespace OcspResponder.Responder.Core.Services;

internal sealed class OcspResponderOptions
{
    private TimeSpan _nextUpdateInterval = TimeSpan.FromDays(3);

    public TimeSpan NextUpdateInterval
    {
        get => _nextUpdateInterval;
        set => _nextUpdateInterval = value.TotalHours >= 1 && value.TotalDays < 28
            ? value
            : throw new ArgumentOutOfRangeException(nameof(NextUpdateInterval), value, "Invalid OCSP response next update interval.");
    }
}
