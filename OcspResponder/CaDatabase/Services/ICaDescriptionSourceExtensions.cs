using System;
using System.Security.Cryptography.X509Certificates;

using OcspResponder.CaDatabase.Entities;

namespace OcspResponder.CaDatabase.Services;

public static class ICaDescriptionSourceExtensions
{
    public static CaDescription Get(this ICaDescriptionSource source, X509Certificate2 certificate)
        => source.Fetch(certificate)
        ?? throw new ArgumentOutOfRangeException(nameof(certificate), certificate.Thumbprint, "Missing responder for specified CA certificate.");
}
