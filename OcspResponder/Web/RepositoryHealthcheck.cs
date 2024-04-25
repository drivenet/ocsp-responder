using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Diagnostics.HealthChecks;

using OcspResponder.Core;

using static System.FormattableString;

namespace OcspResponder.Web;

internal sealed class RepositoryHealthcheck : IHealthCheck
{
    private readonly IOcspResponderRepository _repository;
    private readonly TimeProvider _timeProvider;

    public RepositoryHealthcheck(IOcspResponderRepository repository, TimeProvider timeProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var now = _timeProvider.GetUtcNow();
            var nextUpdateTask = _repository.GetNextUpdate();
            var certificates = await _repository.GetIssuerCertificates();
            foreach (var certificate in certificates)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var chain = await _repository.GetChain(certificate);
                if (chain.Length < 1)
                {
                    return HealthCheckResult.Unhealthy(Invariant($"Invalid chain for issuer with thumbprint {certificate.Thumbprint}({certificate.Subject})."));
                }
            }

            var nextUpdate = await nextUpdateTask;
            if (nextUpdate < now)
            {
                return HealthCheckResult.Unhealthy(Invariant($"Next update {nextUpdate:o} is less than current time {now:o}."));
            }
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Healthy("Repository check was canceled.");
        }

        return HealthCheckResult.Healthy("Repository check completed.");
    }
}
