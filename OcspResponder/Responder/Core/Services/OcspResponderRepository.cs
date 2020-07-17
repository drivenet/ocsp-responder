using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;

using OcspResponder.CaDatabase.Services;
using OcspResponder.Common;
using OcspResponder.Core;

namespace OcspResponder.Responder.Core.Services
{
    internal sealed class OcspResponderRepository : IOcspResponderRepository
    {
        private readonly ICaDescriptionSource _caDescriptions;
        private readonly IOptionsMonitor<OcspResponderOptions> _options;

        public OcspResponderRepository(ICaDescriptionSource caDescriptions, IOptionsMonitor<OcspResponderOptions> options)
        {
            _caDescriptions = caDescriptions ?? throw new ArgumentNullException(nameof(caDescriptions));
            _options = options ?? throw new ArgumentNullException(nameof(options));
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

        public Task<DateTimeOffset> GetNextUpdate() => Task.FromResult(DateTimeOffset.UtcNow + _options.CurrentValue.NextUpdateInterval);

        public Task<AsymmetricAlgorithm> GetResponderPrivateKey(X509Certificate2 caCertificate)
        {
            var description = _caDescriptions.Get(caCertificate);
            return Task.FromResult(description.ResponderPrivateKey);
        }

        public Task<CaCompromisedStatus> IsCaCompromised(X509Certificate2 caCertificate)
        {
            _caDescriptions.Get(caCertificate);
            return Task.FromResult(new CaCompromisedStatus());
        }

        public Task<bool> SerialExists(string serial, X509Certificate2 issuerCertificate)
        {
            var exists = _caDescriptions.Fetch(issuerCertificate)?.Fetch(CertificateUtils.GetSerialNumber(serial)) is object;
            return Task.FromResult(exists);
        }

        public Task<CertificateRevocationStatus> SerialIsRevoked(string serial, X509Certificate2 issuerCertificate)
        {
            var revokedOn = _caDescriptions.Fetch(issuerCertificate)?.Fetch(CertificateUtils.GetSerialNumber(serial))?.RevokedOn;
            var status = revokedOn is { } date
                ? new CertificateRevocationStatus
                {
                    IsRevoked = true,
                    RevokedInfo = new RevokedInfo { Date = date, Reason = RevocationReason.Unspecified },
                }
                : new CertificateRevocationStatus();

            return Task.FromResult(status);
        }
    }
}
