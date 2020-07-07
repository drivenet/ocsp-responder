using System;
using System.Collections.Generic;

namespace OcspResponder.Core
{
    public interface ICaDescriptionUpdater
    {
        IDisposable Update(IReadOnlyCollection<DefaultCaDescription> descriptions);
    }
}
