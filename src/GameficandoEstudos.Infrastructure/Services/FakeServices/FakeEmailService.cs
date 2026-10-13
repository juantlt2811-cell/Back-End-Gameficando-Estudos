using System.Threading.Tasks;
using GameficandoEstudos.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace GameficandoEstudos.Infrastructure.Services.FakeServices
{
    public class FakeEmailService : IEmailService
    {
        private readonly ILogger<FakeEmailService> _logger;

        public FakeEmailService(ILogger<FakeEmailService> logger)
        {
            _logger = logger;
        }

        public Task EnviarEmailRedefinicaoSenhaAsync(string destinatario, string nomeUsuario, string token)
        {
            _logger.LogWarning(
                "[FakeEmailService] Token de redefinicao para {Email}: {Token} (valido por 1 hora)",
                destinatario, token);

            return Task.CompletedTask;
        }
    }
}