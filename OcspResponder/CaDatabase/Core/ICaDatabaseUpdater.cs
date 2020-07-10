using System.Collections.Generic;

namespace OcspResponder.CaDatabase.Core
{
    public interface ICaDatabaseUpdater
    {
        void Update(IReadOnlyCollection<DefaultCaDescription> descriptions);
    }
}
