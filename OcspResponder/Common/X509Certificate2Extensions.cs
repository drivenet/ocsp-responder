﻿using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace OcspResponder.Common;

public static class X509Certificate2Extensions
{
    private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    // Unsecure intentionally since export/import is done in-memory and password is auto-generated
    private static readonly PbeParameters ExportParameters = new(PbeEncryptionAlgorithm.Aes128Cbc, HashAlgorithmName.SHA1, 1);

    public static RSA GetRSACngPrivateKey(this X509Certificate2 certificate)
    {
        if (certificate.GetRSAPrivateKey() is not { } privateKey)
        {
            throw new ArgumentException("Missing certificate private key.", nameof(certificate));
        }

        var keyBytes = new byte[64];
        _rng.GetBytes(keyBytes);
        var exportedKey = privateKey.ExportEncryptedPkcs8PrivateKey(keyBytes, ExportParameters);
        var result = RSA.Create();
        try
        {
            result.ImportEncryptedPkcs8PrivateKey(keyBytes, exportedKey, out _);
        }
        catch
        {
            result.Dispose();
            throw;
        }

        return result;
    }
}
