using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace GameficandoEstudos.Api.Health;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _provider;

    public RedisHealthCheck(IServiceProvider provider)
    {
        _provider = provider;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var redis = _provider.GetService<IConnectionMultiplexer>();
        if (redis is null)
            return HealthCheckResult.Healthy("Redis nao configurado (estado em memoria).");

        try
        {
            await redis.GetDatabase().PingAsync();
            return HealthCheckResult.Healthy("Redis respondeu.");
        }
        catch (Exception ex)
        {
            // Degraded: a API continua funcionando com fallback local.
            return HealthCheckResult.Degraded("Redis indisponivel.", ex);
        }
    }
}
