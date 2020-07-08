using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

using OcspResponder.CaDatabase.Entities;
using OcspResponder.CaDatabase.Services;
using OcspResponder.Common;

namespace OcspResponder.CaDatabase.Core.Infrastructure
{
    internal sealed class CaDatabaseStore : ICaDescriptionSource, ICaDatabaseUpdater, IDisposable
    {
        private IReadOnlyDictionary<X509Certificate2, DefaultCaDescription> _store = new Dictionary<X509Certificate2, DefaultCaDescription>();

        public IEnumerable<X509Certificate2> CaCertificates => _store.Select(pair => pair.Value.CaCertificate);

        public IDisposable Update(IReadOnlyCollection<DefaultCaDescription> descriptions)
        {
            var store = descriptions.ToDictionary(description => description.CaCertificate);
            var oldDescriptions = Interlocked.Exchange(ref _store, store).Values;
            return new DisposableEnumerable(oldDescriptions);
        }

        public CaDescription? Fetch(X509Certificate2 certificate)
        {
            _store.TryGetValue(certificate, out var description);
            return description;
        }

        public void Dispose()
        {
            foreach (var pair in _store.Values)
            {
                pair.Dispose();
            }
        }
    }
}
