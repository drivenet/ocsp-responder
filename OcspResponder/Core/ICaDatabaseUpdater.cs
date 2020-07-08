using System;
using System.Collections.Generic;

namespace OcspResponder.Core
{
    public interface ICaDatabaseUpdater
    {
        IDisposable Update(IReadOnlyCollection<DefaultCaDescription> descriptions);
    }
}
