using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace OcspResponder.Implementation
{
    internal sealed class CaDescription : IDisposable
    {
        public CaDescription(X509Certificate2 caCertificate, X509Certificate2 responderCertificate, AsymmetricAlgorithm responderPrivateKey)
        {
            CaCertificate = caCertificate ?? throw new ArgumentNullException(nameof(caCertificate));
            ResponderCertificate = responderCertificate ?? throw new ArgumentNullException(nameof(responderCertificate));
            ResponderPrivateKey = responderPrivateKey ?? throw new ArgumentNullException(nameof(responderPrivateKey));
        }

        public X509Certificate2 CaCertificate { get; }

        public X509Certificate2 ResponderCertificate { get; }

        public AsymmetricAlgorithm ResponderPrivateKey { get; }

        public void Dispose()
        {
            ResponderPrivateKey.Dispose();
            ResponderCertificate.Dispose();
            CaCertificate.Dispose();
        }
    }
}
