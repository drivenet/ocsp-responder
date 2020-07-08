using System;
using System.Collections.Generic;

namespace OcspResponder.CaDatabase.Core
{
    public interface ICaDatabaseUpdater
    {
        IDisposable Update(IReadOnlyCollection<DefaultCaDescription> descriptions);
    }
}
