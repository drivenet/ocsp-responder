using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OcspResponder.Core;
using OcspResponder.Responder.Services;

namespace OcspResponder.Responder.Core.Services
{
    internal sealed class LoggingOcspResponderEx : IOcspResponderEx
    {
        private readonly IOcspResponderEx _inner;
        private readonly ILogger _logger;

        public LoggingOcspResponderEx(IOcspResponderEx inner, ILogger<IOcspResponder> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OcspHttpResponse> Respond(OcspHttpRequest httpRequest, RequestMetadata metadata)
        {
            _logger.LogInformation(EventIds.Request, "OCSP request {Method} {Uri} from {RemoteIP}, length: {Length}", httpRequest.HttpMethod, metadata.RemoteIP, httpRequest.RequestUri, httpRequest.Content?.Length);
            var response = await _inner.Respond(httpRequest, metadata);
            _logger.LogInformation(EventIds.Response, "OCSP response {Status}, length: {Length}", response.Status, response.Content.Length);
            return response;
        }

        private static class EventIds
        {
            public static readonly EventId Request = new EventId(1, nameof(Request));
            public static readonly EventId Response = new EventId(2, nameof(Response));
        }
    }
}
