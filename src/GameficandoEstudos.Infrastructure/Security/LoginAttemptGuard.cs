using GameficandoEstudos.Application.Interfaces.Security;
using Microsoft.Extensions.Caching.Memory;

namespace GameficandoEstudos.Infrastructure.Security
{
    public class LoginAttemptGuard : ILoginAttemptGuard
    {
        private readonly IMemoryCache _cache;
        private const int LimiteTentativas = 5;
        private static readonly TimeSpan JanelaBloqueio = TimeSpan.FromMinutes(15);

        public LoginAttemptGuard(IMemoryCache cache)
        {
            _cache = cache;
        }

        private static string Chave(string email) =>
            $"login_falhas:{email.Trim().ToLowerInvariant()}";

        public bool EstaBloqueado(string email) =>
            _cache.TryGetValue(Chave(email), out int tentativas) && tentativas >= LimiteTentativas;

        public void RegistrarFalha(string email)
        {
            var chave = Chave(email);
            var tentativasAtuais = _cache.TryGetValue(chave, out int valor) ? valor : 0;
            _cache.Set(chave, tentativasAtuais + 1, JanelaBloqueio);
        }

        public void RegistrarSucesso(string email) =>
            _cache.Remove(Chave(email));
    }
}