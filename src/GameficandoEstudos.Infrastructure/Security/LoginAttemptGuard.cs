using GameficandoEstudos.Application.Interfaces.Security;
using Microsoft.Extensions.Caching.Memory;

namespace GameficandoEstudos.Infrastructure.Security
{
    /// <summary>
    /// Controle de tentativas de login em memoria (por instancia da API).
    /// Com Redis configurado, serve apenas de fallback do RedisLoginAttemptGuard.
    /// </summary>
    public class LoginAttemptGuard : ILoginAttemptGuard
    {
        private readonly IMemoryCache _cache;
        public const int LimiteTentativas = 5;
        public static readonly TimeSpan JanelaBloqueio = TimeSpan.FromMinutes(15);

        public LoginAttemptGuard(IMemoryCache cache)
        {
            _cache = cache;
        }

        public static string NormalizarEmail(string email) => email.Trim().ToLowerInvariant();

        private static string Chave(string email) =>
            $"login_falhas:{NormalizarEmail(email)}";

        public Task<bool> EstaBloqueadoAsync(string email) =>
            Task.FromResult(_cache.TryGetValue(Chave(email), out int tentativas) && tentativas >= LimiteTentativas);

        public Task RegistrarFalhaAsync(string email)
        {
            var chave = Chave(email);
            var tentativasAtuais = _cache.TryGetValue(chave, out int valor) ? valor : 0;

            _cache.Set(chave, tentativasAtuais + 1, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = JanelaBloqueio,
                Size = 1
            });

            return Task.CompletedTask;
        }

        public Task RegistrarSucessoAsync(string email)
        {
            _cache.Remove(Chave(email));
            return Task.CompletedTask;
        }
    }
}
