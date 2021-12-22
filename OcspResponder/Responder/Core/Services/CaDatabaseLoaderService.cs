using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using OcspResponder.CaDatabase.Services;

namespace OcspResponder.Responder.Core.Services;

internal sealed class CaDatabaseLoaderService : IHostedService, IDisposable
{
    private readonly ICaDatabaseLoader _loader;
    private readonly IOptionsMonitor<CaDatabaseLoaderOptions> _options;
    private readonly Timer _timer;
    private readonly TaskCompletionSource<bool> _tcs = new();
    private TimeSpan _currentInterval;

    public CaDatabaseLoaderService(ICaDatabaseLoader loader, IOptionsMonitor<CaDatabaseLoaderOptions> options)
    {
        _loader = loader ?? throw new ArgumentNullException(nameof(loader));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _timer = new Timer(Process);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _currentInterval = GetInterval(_options.CurrentValue);
        _timer.Change(TimeSpan.Zero, _currentInterval);
        _options.OnChange(options =>
        {
            var interval = GetInterval(options);
            if (_currentInterval != interval)
            {
                _timer.Change(interval, interval);
            }
        });
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

    private static TimeSpan GetInterval(CaDatabaseLoaderOptions options)
        => options.LoadInterval != TimeSpan.Zero ? options.LoadInterval : Timeout.InfiniteTimeSpan;

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
