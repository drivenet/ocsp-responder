using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using OcspResponder.Core;

namespace OcspResponder.Composition
{
    internal sealed class PreheatingService : IHostedService
    {
        public PreheatingService(IOcspResponder ocspResponder)
        {
            if (ocspResponder is null)
            {
                throw new ArgumentNullException(nameof(ocspResponder));
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                //PREHEAT
            });
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
