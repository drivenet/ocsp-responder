using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OcspResponder.Implementation
{
    internal sealed class BackgroundUpdaterService : IHostedService, IDisposable
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(10);

        private readonly CaDescriptionUpdater _updater;
        private readonly ILogger _logger;
        private readonly Timer _timer;
        private readonly TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

        public BackgroundUpdaterService(CaDescriptionUpdater updater, ILogger<BackgroundUpdaterService> logger)
        {
            _updater = updater ?? throw new ArgumentNullException(nameof(updater));
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
        }

        private void Update()
        {
            try
            {
                _updater.Update();
                _tcs.TrySetResult(true);
            }
#pragma warning disable CA1031 // Do not catch general exception types -- required for robustness
            catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                _logger.LogError(EventIds.UpdateFailed, exception, "Failed to update CA description.");
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
            public static readonly EventId UpdateFailed = new EventId(1, nameof(UpdateFailed));
        }
    }
}
