using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using OcspResponder.Core;

namespace OcspResponder.Implementation
{
    internal sealed class OcspResponderRepository : IOcspResponderRepository
    {
        private static readonly Task<CaCompromisedStatus> CaStatus = Task.FromResult(new CaCompromisedStatus());
        private static readonly TimeSpan ResponseLifetime = TimeSpan.FromDays(7);

        private readonly ICaDescriptionSource _caDescriptions;

        public OcspResponderRepository(ICaDescriptionSource caDescriptions)
        {
            _caDescriptions = caDescriptions ?? throw new ArgumentNullException(nameof(caDescriptions));
        }

        public void Dispose()
        {
        }

        public Task<X509Certificate2[]> GetChain(X509Certificate2 issuerCertificate)
            => Task.FromResult(
                _caDescriptions.Fetch(issuerCertificate) is { } responder
                    ? new X509Certificate2[] { responder.ResponderCertificate }
                    : Array.Empty<X509Certificate2>());

        public Task<IEnumerable<X509Certificate2>> GetIssuerCertificates() => Task.FromResult(_caDescriptions.CaCertificates);

        public Task<DateTimeOffset> GetNextUpdate() => Task.FromResult(DateTimeOffset.UtcNow + ResponseLifetime);

        public Task<AsymmetricAlgorithm> GetResponderPrivateKey(X509Certificate2 caCertificate)
        {
            var responder = _caDescriptions.Get(caCertificate);
            return Task.FromResult(responder.ResponderPrivateKey);
        }

        public Task<CaCompromisedStatus> IsCaCompromised(X509Certificate2 caCertificate) => CaStatus;

        public Task<bool> SerialExists(string serial, X509Certificate2 issuerCertificate)
        {
            //FIXME: validate cert db
            return Task.FromResult(true);
        }

        public Task<CertificateRevocationStatus> SerialIsRevoked(string serial, X509Certificate2 issuerCertificate)
        {
            //FIXME: validate cert db
            return Task.FromResult(new CertificateRevocationStatus());
        }
    }
}
