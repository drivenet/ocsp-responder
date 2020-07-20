using System;
using System.Threading.Tasks;

using OcspResponder.Core;
using OcspResponder.Responder.Services;

namespace OcspResponder.Responder.Core.Services
{
    internal sealed class OcspResponderEx : IOcspResponderEx
    {
        private readonly IOcspResponder _responder;

        public OcspResponderEx(IOcspResponder responder)
        {
            _responder = responder ?? throw new ArgumentNullException(nameof(responder));
        }

        public Task<OcspHttpResponse> Respond(OcspHttpRequest httpRequest, RequestMetadata metadata) => _responder.Respond(httpRequest);
    }
}
