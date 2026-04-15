using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Products.Api.Health
{
    public static class HealthDependencyInjection
    {
        public static IServiceCollection AddHealthServices(this IServiceCollection services, IConfiguration config)
        {
            var healthChecks = services.AddHealthChecks();

            //  Liveness (App alive hai ya nahi)
            healthChecks.AddCheck(
                "self",
                () => HealthCheckResult.Healthy(),
                tags: new[] { "live" });

            //  Memory Check (custom)
            healthChecks.AddCheck<MemoryHealthCheck>(
                "memory",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { "ready" });

            //  SQL Server
            healthChecks.AddSqlServer(
                connectionString: config.GetConnectionString("DefaultConnection")!,
                name: "sql",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { "ready", "db" });

         
            // Health UI
            services.AddHealthChecksUI(options =>
            {
                options.SetEvaluationTimeInSeconds(30);
                options.MaximumHistoryEntriesPerEndpoint(60);
                options.AddHealthCheckEndpoint("API Health", "/health/details");
            })
            .AddInMemoryStorage();

            return services;
        }
    }
}
