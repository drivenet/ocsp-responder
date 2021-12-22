namespace OcspResponder.Responder.Core;

public interface IMetricRecorder
{
    void RecordRequest();

    void RecordError();
}
