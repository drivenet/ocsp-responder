namespace OcspResponder.Responder.Services
{
    public interface IMetricReader
    {
        ulong Requests { get; }

        ulong Errors { get; }
    }
}
