using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace OcspResponder.Implementation
{
    internal abstract class CaDescription
    {
        public abstract X509Certificate2 CaCertificate { get; }

        public abstract X509Certificate2 ResponderCertificate { get; }

        public abstract AsymmetricAlgorithm ResponderPrivateKey { get; }

        public abstract CertificateRecord? Fetch(BigInteger serial);
    }
}
