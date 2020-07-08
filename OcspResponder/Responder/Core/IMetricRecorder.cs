using OcspResponder.Core;

namespace OcspResponder.Responder.Core
{
    public interface IMetricRecorder
    {
        void Record(OcspHttpRequest request);

        void Record(OcspHttpResponse response);
    }
}
