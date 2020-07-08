using System;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OcspResponder.Core;

namespace OcspResponder.Responder.Core.Services
{
    internal sealed class LoggingOcspResponder : IOcspResponder
    {
        private readonly IOcspResponder _inner;
        private readonly ILogger _logger;

        public LoggingOcspResponder(IOcspResponder inner, ILogger<IOcspResponder> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<OcspHttpResponse> Respond(OcspHttpRequest httpRequest)
        {
            _logger.LogInformation(EventIds.Request, "OCSP request {Method} {Uri}, length: {Length}", httpRequest.HttpMethod, httpRequest.RequestUri, httpRequest.Content?.Length);
            var response = await _inner.Respond(httpRequest);
            _logger.LogInformation(EventIds.Response, "OCSP Response {Status}, length: {Length}", response.Status, response.Content.Length);
            return response;
        }

        private static class EventIds
        {
            public static readonly EventId Request = new EventId(1, nameof(Request));
            public static readonly EventId Response = new EventId(2, nameof(Response));
        }
    }
}
