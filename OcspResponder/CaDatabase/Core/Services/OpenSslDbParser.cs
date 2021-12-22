using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Numerics;

using OcspResponder.CaDatabase.Entities;

using Org.BouncyCastle.Utilities.Encoders;

using static System.FormattableString;

namespace OcspResponder.CaDatabase.Core.Services
{
    internal sealed class OpenSslDbParser
    {
#pragma warning disable CA1822 // Mark members as static -- future-proofing
        public IReadOnlyList<CertificateRecord> Parse(TextReader reader, DateTimeOffset now)
#pragma warning restore CA1822 // Mark members as static
        {
            var records = new List<CertificateRecord>();
            var lineNumber = 0;
            while (reader.ReadLine() is { } line)
            {
                ++lineNumber;
                CertificateRecord? record;
                try
                {
                    record = Parse(line, now);
                }
                catch (ArgumentException exception)
                {
                    throw new FormatException(Invariant($"Invalid certificate record at line {lineNumber}."), exception);
                }

                if (record is object)
                {
                    records.Add(record);
                }
            }

            return records;
        }

        private static CertificateRecord? Parse(string line, DateTimeOffset now)
        {
            // Don't bother parsing expired certs
            if (line.StartsWith("E ", StringComparison.Ordinal))
            {
                return null;
            }

            var parts = line.Split('\t');
            var length = parts.Length;
            if (length != 6)
            {
                throw new ArgumentOutOfRangeException(nameof(line), length, "Invalid number of columns in certificate record.");
            }

            if (parts[4] != "unknown")
            {
                throw new ArgumentOutOfRangeException(nameof(line), "Unexpected certificate file name encountered.");
            }

            DateTimeOffset? revokedOn;
            var type = parts[0];
            var typeLength = type.Length;
            if (typeLength != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(line), typeLength, "Invalid certificate record type length.");
            }

            switch (type)
            {
                case "V":
                    if (parts[2].Length != 0)
                    {
                        throw new ArgumentOutOfRangeException(nameof(line), "Unexpected revocation timestamp encountered for valid certificate.");
                    }

                    var expirationTimestamp = ParseTimestamp(parts[1]);
                    if (now >= expirationTimestamp)
                    {
                        return null;
                    }

                    revokedOn = null;
                    break;

                case "R":
                    revokedOn = ParseTimestamp(parts[2]);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(line), type, "Invalid certificate record type.");
            }

            byte[] serialBytes;
            try
            {
                serialBytes = Hex.Decode(parts[3]);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("Invalid certificate serial string.", nameof(line), exception);
            }

            var serial = new BigInteger(serialBytes, isUnsigned: true, isBigEndian: true);
            return new CertificateRecord(serial, revokedOn);
        }

        private static DateTime ParseTimestamp(string timestampString)
        {
            if (!timestampString.EndsWith('Z')
                || !ulong.TryParse(timestampString.AsSpan(0, timestampString.Length - 1), NumberStyles.None, NumberFormatInfo.InvariantInfo, out var expirationTicks))
            {
                throw new ArgumentOutOfRangeException(nameof(timestampString), timestampString, "Invalid timestamp in certificate record.");
            }

            var year = checked((int)(2000 + (expirationTicks / 10000000000)));
            var month = unchecked((int)((expirationTicks / 100000000) % 100));
            var day = unchecked((int)((expirationTicks / 1000000) % 100));
            var hour = unchecked((int)((expirationTicks / 10000) % 100));
            var minute = unchecked((int)((expirationTicks / 100) % 100));
            var second = unchecked((int)(expirationTicks % 100));
            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc);
        }
    }
}
