using System.Threading.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;

namespace GameficandoEstudos.Infrastructure.RateLimiting
{
    public static class RedisRateLimitPartitions
    {
        /// <summary>
        /// Particao de janela fixa. Com Redis configurado o contador e compartilhado entre
        /// replicas; sem Redis, usa o limiter em memoria padrao do ASP.NET Core.
        /// </summary>
        public static RateLimitPartition<string> JanelaFixa(
            IServiceProvider services, string politica, string chave, int limite, TimeSpan janela)
        {
            var redis = services.GetService<IConnectionMultiplexer>();

            if (redis is null)
            {
                return RateLimitPartition.GetFixedWindowLimiter(chave, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = limite,
                    Window = janela,
                    QueueLimit = 0
                });
            }

            var logger = services.GetService<ILoggerFactory>()?.CreateLogger<RedisFixedWindowRateLimiter>()
                         ?? (ILogger)NullLogger.Instance;

            return RateLimitPartition.Get(chave, k =>
                new RedisFixedWindowRateLimiter(redis, $"gameficando:rl:{politica}:{k}", limite, janela, logger));
        }
    }
}
