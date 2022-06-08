using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using OcspResponder.CaDatabase.Services;

namespace OcspResponder.Web;

internal sealed class CaDescriptionsHealthcheck : IHealthCheck
{
    private readonly ICaDescriptionSource _source;

    public CaDescriptionsHealthcheck(ICaDescriptionSource source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            foreach (var certificate in _source.CaCertificates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                _source.Get(certificate);
            }
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(HealthCheckResult.Healthy("CA descriptions check was canceled."));
        }

        return Task.FromResult(HealthCheckResult.Healthy("CA descriptions check completed."));
    }
}
