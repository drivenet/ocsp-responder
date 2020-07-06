using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using static System.FormattableString;

namespace OcspResponder.Implementation
{
    internal sealed class ResponderChainLoader
    {
        public (X509Certificate2 CaCertificate, X509Certificate2 ResponderCertificate) Load(string fileName, string password)
        {
            var certificates = new X509Certificate2Collection();
            try
            {
                certificates.Import(
                    fileName,
                    password,
                    X509KeyStorageFlags.EphemeralKeySet | X509KeyStorageFlags.UserKeySet | X509KeyStorageFlags.Exportable);

                var count = certificates.Count;
                if (count != 2)
                {
                    throw new ArgumentOutOfRangeException(nameof(fileName), count, "The certificate file must contain exactly 2 certificates.");
                }

                var caCertificate = certificates[0];
                ValidateCaCertificate(certificates[0]);
                var responderCertificate = certificates[1];
                ValidateResponderCertificate(responderCertificate, caCertificate);

                return (caCertificate, responderCertificate);
            }
            catch (Exception exception)
            {
                foreach (var certificate in certificates)
                {
                    certificate.Dispose();
                }

                throw new InvalidDataException(Invariant($"Failed to load certificates from file \"{fileName}\"."), exception);
            }
        }

        private static void ValidateCaCertificate(X509Certificate2 caCertificate)
        {
            var issuerExtensions = caCertificate.Extensions;
            var isCertificateAuthority = issuerExtensions.OfType<X509BasicConstraintsExtension>().SingleOrDefault()?.CertificateAuthority ?? false;
            if (!isCertificateAuthority)
            {
                throw new ArgumentOutOfRangeException(nameof(caCertificate), caCertificate.Thumbprint, "The certificate does not represent a CA.");
            }

            var keyUsages = issuerExtensions.OfType<X509KeyUsageExtension>().SingleOrDefault()?.KeyUsages ?? X509KeyUsageFlags.None;
            if ((keyUsages & (X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign)) != (X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.CrlSign))
            {
                throw new ArgumentOutOfRangeException(nameof(caCertificate), caCertificate.Thumbprint, "The certificate does not represent a key+CRL signing CA.");
            }

            caCertificate.Verify();
        }

        private static void ValidateResponderCertificate(X509Certificate2 responderCertificate, X509Certificate2 caCertificate)
        {
            if (responderCertificate.Issuer != caCertificate.Subject)
            {
                throw new ArgumentOutOfRangeException(nameof(caCertificate), responderCertificate.Thumbprint, "The responder certificate was not issued by CA represented by the first one.");
            }

            var responderExtensions = responderCertificate.Extensions;
            var isCertificateAuthority = responderExtensions.OfType<X509BasicConstraintsExtension>().SingleOrDefault()?.CertificateAuthority ?? false;
            if (isCertificateAuthority)
            {
                throw new ArgumentOutOfRangeException(nameof(caCertificate), responderCertificate.Thumbprint, "The responder certificate represents a CA.");
            }

            var keyUsages = responderExtensions.OfType<X509KeyUsageExtension>().SingleOrDefault()?.KeyUsages ?? X509KeyUsageFlags.None;
            if ((keyUsages & X509KeyUsageFlags.DigitalSignature) != X509KeyUsageFlags.DigitalSignature)
            {
                throw new ArgumentOutOfRangeException(nameof(caCertificate), responderCertificate.Thumbprint, "The responder certificate cannot produce digital signatures.");
            }

            var ekus = (responderExtensions.OfType<X509EnhancedKeyUsageExtension>().SingleOrDefault()?.EnhancedKeyUsages ?? new OidCollection()).Cast<Oid>();
            if (!ekus.Any(eku => eku.FriendlyName == "OCSP Signing"))
            {
                throw new ArgumentOutOfRangeException(nameof(caCertificate), responderCertificate.Thumbprint, "The responder certificate cannot sign OCSP responses.");
            }
        }
    }
}
