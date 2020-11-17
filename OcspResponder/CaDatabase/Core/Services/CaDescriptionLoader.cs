using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

using OcspResponder.CaDatabase.Entities;
using OcspResponder.Common;

namespace OcspResponder.CaDatabase.Core.Services
{
    internal sealed class CaDescriptionLoader
    {
        private readonly OpenSslDbParser _dbParser;
        private readonly ResponderChainLoader _chainLoader;

        public CaDescriptionLoader(OpenSslDbParser dbParser, ResponderChainLoader chainLoader)
        {
            _dbParser = dbParser ?? throw new ArgumentNullException(nameof(dbParser));
            _chainLoader = chainLoader ?? throw new ArgumentNullException(nameof(chainLoader));
        }

        public DefaultCaDescription Load(CaDescriptionPaths paths, DateTimeOffset now)
        {
            var records = LoadRecords(paths.DbFilePath, now);
            var (caCertificate, responderCertificate) = _chainLoader.Load(paths.CertFilePath);
            try
            {
                var responderPrivateKey = responderCertificate.GetRSACngPrivateKey();

                return new DefaultCaDescription(caCertificate, responderCertificate, responderPrivateKey, records);
            }
            catch
            {
                caCertificate.Dispose();
                responderCertificate.Dispose();
                throw;
            }
        }

        private IReadOnlyDictionary<BigInteger, CertificateRecord> LoadRecords(string dbFileName, DateTimeOffset now)
        {
            IReadOnlyList<CertificateRecord> records;
            using (var dbFile = new StreamReader(File.Open(dbFileName, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.ASCII, false, 256))
            {
                records = _dbParser.Parse(dbFile, now);
            }

            return records.ToDictionary(record => record.Serial);
        }
    }
}
