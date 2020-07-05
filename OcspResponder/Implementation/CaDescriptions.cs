using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace OcspResponder.Implementation
{
    internal sealed class CaDescriptions : IDisposable, ICaDescriptionSource
    {
        private readonly ConcurrentDictionary<X509Certificate2, CaDescription> _responders = new ConcurrentDictionary<X509Certificate2, CaDescription>();

        public IEnumerable<X509Certificate2> CaCertificates => _responders.Select(pair => pair.Value.CaCertificate);

        public bool TryAdd(CaDescription description) => _responders.TryAdd(description.CaCertificate, description);

        public CaDescription Get(X509Certificate2 caCertificate)
        {
            if (!_responders.TryGetValue(caCertificate, out var description))
            {
                throw new ArgumentOutOfRangeException(nameof(caCertificate), caCertificate.Thumbprint, "Missing responder for specified CA certificate.");
            }

            return description;
        }

        public CaDescription? Fetch(X509Certificate2 certificate)
        {
            _responders.TryGetValue(certificate, out var description);
            return description;
        }

        public void Dispose()
        {
            foreach (var pair in _responders)
            {
                pair.Value.Dispose();
            }
        }
    }
}
