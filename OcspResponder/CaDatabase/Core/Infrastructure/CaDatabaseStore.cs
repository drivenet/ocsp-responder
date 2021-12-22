using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

using OcspResponder.CaDatabase.Entities;
using OcspResponder.CaDatabase.Services;

namespace OcspResponder.CaDatabase.Core.Infrastructure;

internal sealed class CaDatabaseStore : ICaDescriptionSource, ICaDatabaseUpdater, IDisposable
{
    private readonly object _lock = new();
    private Dictionary<X509Certificate2, DefaultCaDescription> _store = new();
    private Dictionary<X509Certificate2, DefaultCaDescription> _prevStore = new();

    public IEnumerable<X509Certificate2> CaCertificates => _store.Select(pair => pair.Value.CaCertificate);

    public void Update(IReadOnlyCollection<DefaultCaDescription> descriptions)
    {
        var store = descriptions.ToDictionary(description => description.CaCertificate);
        lock (_lock)
        {
            _prevStore = _store;
            _store = store;
            foreach (var certificate in _store.Keys)
            {
                if (_prevStore.Remove(certificate, out var description))
                {
                    description.Dispose();
                }
            }
        }
    }

    public CaDescription? Fetch(X509Certificate2 certificate)
    {
        if (!_store.TryGetValue(certificate, out var description))
        {
            _prevStore.TryGetValue(certificate, out description);
        }

        return description;
    }

    public void Dispose()
    {
        lock (_lock)
        {
            try
            {
                foreach (var pair in _store.Values)
                {
                    pair.Dispose();
                }
            }
            finally
            {
                _store.Clear();
            }

            try
            {
                foreach (var pair in _prevStore.Values)
                {
                    pair.Dispose();
                }
            }
            finally
            {
                _prevStore.Clear();
            }
        }
    }
}
