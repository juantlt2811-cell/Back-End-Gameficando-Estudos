using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace GameficandoEstudos.Infrastructure.Redis;

public static class RedisServiceCollectionExtensions
{
    /// <summary>
    /// Registra a conexao com o Redis se 'Redis:ConnectionString' estiver configurada.
    /// Sem a configuracao, nada e registrado e a API continua usando estado em memoria.
    /// </summary>
    public static IServiceCollection AddRedisOpcional(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration["Redis:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
            return services;

        var options = ConfigurationOptions.Parse(connectionString);

        // A API deve subir mesmo com o Redis fora do ar; a conexao se refaz sozinha depois.
        options.AbortOnConnectFail = false;
        options.ConnectTimeout = 2000;
        options.SyncTimeout = 1000;
        options.AsyncTimeout = 1000;
        options.ClientName = "gameficando-api";

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options));
        return services;
    }
}
