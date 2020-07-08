using System;
using System.Threading.Tasks;

namespace OcspResponder.Core.Services
{
    internal sealed class MetricsCollectingOcspResponder : IOcspResponder
    {
        private readonly IOcspResponder _inner;
        private readonly IMetricRecorder _recorder;

        public MetricsCollectingOcspResponder(IOcspResponder inner, IMetricRecorder recorder)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _recorder = recorder ?? throw new ArgumentNullException(nameof(recorder));
        }

        public async Task<OcspHttpResponse> Respond(OcspHttpRequest httpRequest)
        {
            _recorder.Record(httpRequest);
            var response = await _inner.Respond(httpRequest);
            _recorder.Record(response);
            return response;
        }
    }
}
