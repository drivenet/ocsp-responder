using System;
using System.Net;

namespace OcspResponder.Responder.Services
{
    public sealed class RequestMetadata
    {
        public RequestMetadata(IPAddress remoteIP)
        {
            RemoteIP = remoteIP ?? throw new ArgumentNullException(nameof(remoteIP));
        }

        public IPAddress RemoteIP { get; }
    }
}
