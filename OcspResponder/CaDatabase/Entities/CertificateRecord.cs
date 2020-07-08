using System;
using System.Numerics;

namespace OcspResponder.CaDatabase.Entities
{
    public sealed class CertificateRecord
    {
        public CertificateRecord(BigInteger serial, DateTimeOffset? revokedOn)
        {
            Serial = serial;
            if (revokedOn is { } revocationTimestamp)
            {
                revocationTimestamp = revocationTimestamp.ToUniversalTime();
                if (revocationTimestamp.Year < 1970)
                {
                    throw new ArgumentOutOfRangeException(nameof(revokedOn), revokedOn, "Invalid revocation timestamp.");
                }

                RevokedOn = revocationTimestamp;
            }
        }

        public BigInteger Serial { get; }

        public DateTimeOffset? RevokedOn { get; }
    }
}
