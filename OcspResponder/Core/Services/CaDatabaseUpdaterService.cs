using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

namespace OcspResponder.Core.Services
{
    internal sealed class CaDatabaseUpdaterService : IHostedService, IDisposable
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(17);

        private readonly ICaDatabaseLoader _loader;
        private readonly Timer _timer;
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public CaDatabaseUpdaterService(ICaDatabaseLoader loader)
        {
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _timer = new Timer(Process);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer.Change(TimeSpan.Zero, Interval);
            return Task.WhenAny(_tcs.Task, Task.Delay(Timeout.Infinite, cancellationToken));
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            Dispose(cancellationToken);
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Dispose(CancellationToken.None);
        }

        private void Dispose(CancellationToken cancellationToken)
        {
            do
            {
                var locked = false;
                try
                {
#pragma warning disable CA2002 // Do not lock on objects with weak identity -- local object
                    Monitor.TryEnter(_timer, ref locked);
#pragma warning restore CA2002 // Do not lock on objects with weak identity
                    if (locked)
                    {
                        _timer.Dispose();
                        break;
                    }
                }
                finally
                {
                    if (locked)
                    {
                        Monitor.Exit(_timer);
                    }
                }
            }
            while (!cancellationToken.IsCancellationRequested);
        }

        private void Update()
        {
            try
            {
                _loader.Load();
            }
#pragma warning disable CA1031 // Do not catch general exception types -- required for robustness
            catch
#pragma warning restore CA1031 // Do not catch general exception types
            {
                return;
            }

            _tcs.TrySetResult(true);
        }

        private void Process(object? state)
        {
            var locked = false;
            try
            {
#pragma warning disable CA2002 // Do not lock on objects with weak identity -- local object
                Monitor.TryEnter(_timer, ref locked);
#pragma warning restore CA2002 // Do not lock on objects with weak identity
                if (locked)
                {
                    Update();
                }
            }
            finally
            {
                if (locked)
                {
                    Monitor.Exit(_timer);
                }
            }
        }
    }
}
