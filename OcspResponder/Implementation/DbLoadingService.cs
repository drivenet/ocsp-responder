using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OcspResponder.Implementation
{
    internal sealed class DbLoadingService : IHostedService, IDisposable
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

        private readonly CaDescriptionStore _store;
        private readonly CaDescriptionLoader _loader;
        private readonly ILogger _logger;
        private readonly Timer _timer;
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();
        private IDisposable? _cleanup;

        public DbLoadingService(CaDescriptionStore store, CaDescriptionLoader loader, ILogger<DbLoadingService> logger)
        {
            _store = store ?? throw new ArgumentNullException(nameof(store));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _logger = logger;
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

            _store.Dispose();
            _cleanup?.Dispose();
        }

        private void Update()
        {
            try
            {
                if (_cleanup is { } cleanup)
                {
                    _cleanup = null;
                    cleanup.Dispose();
                }

                var now = DateTimeOffset.UtcNow;
                var dbFileName = "../../elastic-sub-ca.db";
                var certFileName = "../../elastic-sub-ca-ocsp.pfx";
                var password = Path.GetFileNameWithoutExtension(certFileName);
                var description = _loader.Load(dbFileName, certFileName, password, now);
                _cleanup = _store.Update(new[] { description });
                _tcs.SetResult(true);
            }
            catch (Exception exception)
            {
                _logger.LogError(EventIds.LoadFailed, exception, "Failed to load certificate database.");
                throw;
            }
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

        private static class EventIds
        {
            public static readonly EventId LoadFailed = new EventId(1, nameof(LoadFailed));
        }
    }
}
