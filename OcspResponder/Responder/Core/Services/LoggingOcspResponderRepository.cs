using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

using OcspResponder.Common;
using OcspResponder.Core;

namespace OcspResponder.Responder.Core.Services
{
    internal sealed class LoggingOcspResponderRepository : IOcspResponderRepository
    {
        private readonly IOcspResponderRepository _inner;
        private readonly ILogger _logger;

        public LoggingOcspResponderRepository(IOcspResponderRepository inner, ILogger<IOcspResponderRepository> logger)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void Dispose() => _inner.Dispose();

        public Task<X509Certificate2[]> GetChain(X509Certificate2 issuerCertificate) => _inner.GetChain(issuerCertificate);

        public Task<IEnumerable<X509Certificate2>> GetIssuerCertificates() => _inner.GetIssuerCertificates();

        public Task<DateTimeOffset> GetNextUpdate() => _inner.GetNextUpdate();

        public Task<AsymmetricAlgorithm> GetResponderPrivateKey(X509Certificate2 caCertificate) => _inner.GetResponderPrivateKey(caCertificate);

        public Task<CaCompromisedStatus> IsCaCompromised(X509Certificate2 caCertificate) => _inner.IsCaCompromised(caCertificate);

        public async Task<bool> SerialExists(string serial, X509Certificate2 issuerCertificate)
        {
            var exists = await _inner.SerialExists(serial, issuerCertificate);
            if (!exists)
            {
                var serialHex = CertificateUtils.ToSerialNumberHexString(CertificateUtils.GetSerialNumber(serial));
                _logger.LogError(
                    EventIds.MissingCertificate,
                    "Missing certificate with serial {Serial} issued by issuer with thumbprint {IssuerThumbprint}({IssuerSubject})",
                    serialHex,
                    issuerCertificate.Thumbprint,
                    issuerCertificate.Subject);
            }

            return exists;
        }

        public async Task<CertificateRevocationStatus> SerialIsRevoked(string serial, X509Certificate2 issuerCertificate)
        {
            var status = await _inner.SerialIsRevoked(serial, issuerCertificate);
            var serialHex = CertificateUtils.ToSerialNumberHexString(CertificateUtils.GetSerialNumber(serial));
            if (status.IsRevoked)
            {
                _logger.LogWarning(
                    EventIds.InvalidCertificate,
                    "Invalid certificate with serial {Serial}, revoked on {RevokedOn} issued by {IssuerThumbprint}({IssuerSubject})",
                    serialHex,
                    status.RevokedInfo.Date,
                    issuerCertificate.Thumbprint,
                    issuerCertificate.Subject);
            }
            else
            {
                _logger.LogInformation(
                    EventIds.ValidatedCertificate,
                    "Validated certificate with serial {Serial} issued by {IssuerThumbprint}({IssuerSubject})",
                    serialHex,
                    issuerCertificate.Thumbprint,
                    issuerCertificate.Subject);
            }

            return status;
        }

        private static class EventIds
        {
            public static readonly EventId ValidatedCertificate = new EventId(1, nameof(ValidatedCertificate));
            public static readonly EventId MissingCertificate = new EventId(2, nameof(MissingCertificate));
            public static readonly EventId InvalidCertificate = new EventId(3, nameof(InvalidCertificate));
        }
    }
}
