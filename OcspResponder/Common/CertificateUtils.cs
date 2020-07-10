using System.Globalization;
using System.Numerics;

using Org.BouncyCastle.Utilities.Encoders;

namespace OcspResponder.Common
{
    public static class CertificateUtils
    {
        public static BigInteger GetSerialNumber(string serial)
            => BigInteger.Parse(serial, NumberStyles.None, NumberFormatInfo.InvariantInfo);

        public static string ToSerialNumberHexString(BigInteger serial)
            => Hex.ToHexString(serial.ToByteArray(isUnsigned: true, isBigEndian: true));
    }
}
