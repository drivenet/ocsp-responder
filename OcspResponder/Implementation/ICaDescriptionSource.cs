using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace OcspResponder.Implementation
{
    internal interface ICaDescriptionSource
    {
        IEnumerable<X509Certificate2> CaCertificates { get; }

        CaDescription? Fetch(X509Certificate2 certificate);

        CaDescription Get(X509Certificate2 caCertificate);
    }
}
