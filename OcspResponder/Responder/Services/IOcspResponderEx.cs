using System.Threading.Tasks;

using OcspResponder.Core;

namespace OcspResponder.Responder.Services;

public interface IOcspResponderEx
{
    Task<OcspHttpResponse> Respond(OcspHttpRequest httpRequest, RequestMetadata metadata);
}
