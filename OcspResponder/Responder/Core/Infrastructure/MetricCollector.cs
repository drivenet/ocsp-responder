using System.Threading;

using OcspResponder.Responder.Services;

namespace OcspResponder.Responder.Core.Infrastructure
{
    internal sealed class MetricCollector : IMetricRecorder, IMetricReader
    {
        private long _requests;
        private long _errors;

        public ulong Requests => unchecked((ulong)_requests);

        public ulong Errors => unchecked((ulong)_errors);

        public void RecordRequest() => Interlocked.Increment(ref _requests);

        public void RecordError() => Interlocked.Increment(ref _errors);
    }
}
