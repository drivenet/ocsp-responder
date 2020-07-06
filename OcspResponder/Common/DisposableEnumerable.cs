using System;
using System.Collections.Generic;

namespace OcspResponder.Common
{
    public sealed class DisposableEnumerable : IDisposable
    {
        private readonly IEnumerable<IDisposable> _disposables;

        public DisposableEnumerable(IEnumerable<IDisposable> disposables)
        {
            _disposables = disposables ?? throw new ArgumentNullException(nameof(disposables));
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }
    }
}
