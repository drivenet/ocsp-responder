namespace OcspResponder.Core
{
    internal interface IMetricRecorder
    {
        void Record(OcspHttpRequest request);

        void Record(OcspHttpResponse response);
    }
}
