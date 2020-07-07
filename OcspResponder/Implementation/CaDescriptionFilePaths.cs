using System;
using System.IO;

namespace OcspResponder.Implementation
{
    internal sealed class CaDescriptionFilePaths
    {
        public CaDescriptionFilePaths(string dbFilePath, string certFilePath)
        {
            if (!Path.IsPathFullyQualified(dbFilePath))
            {
                throw new ArgumentOutOfRangeException(nameof(dbFilePath), dbFilePath, "The database file path is not fully qualified.");
            }

            if (!Path.IsPathFullyQualified(certFilePath))
            {
                throw new ArgumentOutOfRangeException(nameof(certFilePath), certFilePath, "The certificate file path is not fully qualified.");
            }

            DbFilePath = dbFilePath;
            CertFilePath = certFilePath;
        }

        public string DbFilePath { get; }

        public string CertFilePath { get; }
    }
}
