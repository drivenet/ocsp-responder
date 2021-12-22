using System;

using OcspResponder.Core;

namespace OcspResponder.Responder.Core.Services;

internal sealed class MetricsCollectingOcspLogger : IOcspLogger
{
    private readonly IOcspLogger _inner;
    private readonly IMetricRecorder _recorder;

    public MetricsCollectingOcspLogger(IOcspLogger inner, IMetricRecorder recorder)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
    }

    public void Debug(string message)
    {
        _inner.Debug(message);
    }

    public void Error(string message)
    {
        _recorder.RecordError();
        _inner.Error(message);
    }

    public void Warn(string message)
    {
        _recorder.RecordError();
        _inner.Warn(message);
    }
}
