using GameficandoEstudos.Application.Interfaces.Security;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace GameficandoEstudos.Infrastructure.Security
{
    /// <summary>
    /// Controle de tentativas de login compartilhado entre todas as replicas da API.
    /// Se o Redis estiver indisponivel, cai para a contagem local em memoria.
    /// </summary>
    public class RedisLoginAttemptGuard : ILoginAttemptGuard
    {
        // INCR + PEXPIRE no mesmo script: atomico, entao a chave nunca fica sem expiracao.
        private const string ScriptIncrementar = @"
            local n = redis.call('INCR', KEYS[1])
            redis.call('PEXPIRE', KEYS[1], ARGV[1])
            return n";

        private readonly IConnectionMultiplexer _redis;
        private readonly LoginAttemptGuard _fallback;
        private readonly ILogger<RedisLoginAttemptGuard> _logger;

        public RedisLoginAttemptGuard(
            IConnectionMultiplexer redis,
            LoginAttemptGuard fallback,
            ILogger<RedisLoginAttemptGuard> logger)
        {
            _redis = redis;
            _fallback = fallback;
            _logger = logger;
        }

        private static string Chave(string email) =>
            $"gameficando:login_falhas:{LoginAttemptGuard.NormalizarEmail(email)}";

        public async Task<bool> EstaBloqueadoAsync(string email)
        {
            try
            {
                var valor = await _redis.GetDatabase().StringGetAsync(Chave(email));
                return valor.TryParse(out long tentativas) && tentativas >= LoginAttemptGuard.LimiteTentativas;
            }
            catch (Exception ex) when (ex is RedisException or TimeoutException)
            {
                AvisarFallback(ex);
                return await _fallback.EstaBloqueadoAsync(email);
            }
        }

        public async Task RegistrarFalhaAsync(string email)
        {
            try
            {
                await _redis.GetDatabase().ScriptEvaluateAsync(
                    ScriptIncrementar,
                    new RedisKey[] { Chave(email) },
                    new RedisValue[] { (long)LoginAttemptGuard.JanelaBloqueio.TotalMilliseconds });
            }
            catch (Exception ex) when (ex is RedisException or TimeoutException)
            {
                AvisarFallback(ex);
                await _fallback.RegistrarFalhaAsync(email);
            }
        }

        public async Task RegistrarSucessoAsync(string email)
        {
            try
            {
                await _redis.GetDatabase().KeyDeleteAsync(Chave(email));
            }
            catch (Exception ex) when (ex is RedisException or TimeoutException)
            {
                AvisarFallback(ex);
                await _fallback.RegistrarSucessoAsync(email);
            }
        }

        private void AvisarFallback(Exception ex) =>
            _logger.LogWarning(ex, "Redis indisponivel; usando contagem local de tentativas de login.");
    }
}
