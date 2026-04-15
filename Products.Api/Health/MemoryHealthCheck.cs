using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Products.Api.Health
{
    public class MemoryHealthCheck : IHealthCheck
    {
        private const long Threshold = 500 * 1024 * 1024; // 500 MB

        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            var allocated = GC.GetTotalMemory(false);

            if (allocated < Threshold)
            {
                return Task.FromResult(HealthCheckResult.Healthy($"Memory OK: {allocated} bytes"));
            }

            return Task.FromResult(
                new HealthCheckResult(
                    context.Registration.FailureStatus,
                    $"High memory usage: {allocated} bytes"));
        }
    }
}
