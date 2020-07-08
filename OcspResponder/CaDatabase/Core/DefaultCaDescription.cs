using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using OcspResponder.CaDatabase.Entities;

namespace OcspResponder.CaDatabase.Core
{
    public sealed class DefaultCaDescription : CaDescription, IDisposable
    {
        private readonly IReadOnlyDictionary<BigInteger, CertificateRecord> _records;

        public DefaultCaDescription(
            X509Certificate2 caCertificate,
            X509Certificate2 responderCertificate,
            AsymmetricAlgorithm responderPrivateKey,
            IReadOnlyDictionary<BigInteger, CertificateRecord> records)
        {
            CaCertificate = caCertificate ?? throw new ArgumentNullException(nameof(caCertificate));
            ResponderCertificate = responderCertificate ?? throw new ArgumentNullException(nameof(responderCertificate));
            ResponderPrivateKey = responderPrivateKey ?? throw new ArgumentNullException(nameof(responderPrivateKey));
            _records = records ?? throw new ArgumentNullException(nameof(records));
        }

        public override X509Certificate2 CaCertificate { get; }

        public override X509Certificate2 ResponderCertificate { get; }

        public override AsymmetricAlgorithm ResponderPrivateKey { get; }

        public override CertificateRecord? Fetch(BigInteger serial)
        {
            _records.TryGetValue(serial, out var record);
            return record;
        }

        public void Dispose()
        {
            ResponderPrivateKey.Dispose();
            ResponderCertificate.Dispose();
            CaCertificate.Dispose();
        }
    }
}
