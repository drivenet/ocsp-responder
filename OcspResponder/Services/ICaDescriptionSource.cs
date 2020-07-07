using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

using OcspResponder.Entities;

namespace OcspResponder.Services
{
    public interface ICaDescriptionSource
    {
        IEnumerable<X509Certificate2> CaCertificates { get; }

        CaDescription? Fetch(X509Certificate2 certificate);
    }
}
