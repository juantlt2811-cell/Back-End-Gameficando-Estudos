using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace GameficandoEstudos.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task EnviarEmailRedefinicaoSenhaAsync(string destinatario, string nomeUsuario, string token)
        {
            var mensagem = new MimeMessage();
            mensagem.From.Add(new MailboxAddress(
                _configuration["Smtp:NomeRemetente"],
                _configuration["Smtp:EmailRemetente"]));
            mensagem.To.Add(new MailboxAddress(nomeUsuario, destinatario));
            mensagem.Subject = "Redefinição de senha — Gameficando Estudos";

            var corpo =
                $"Olá, {nomeUsuario}.\n\n" +
                "Recebemos uma solicitação para redefinir sua senha.\n\n" +
                $"Use o código abaixo na tela de redefinição de senha (válido por 1 hora):\n\n{token}\n\n" +
                "Se você não pediu essa redefinição, ignore este e-mail — sua senha continua a mesma.";

            mensagem.Body = new TextPart("plain") { Text = corpo };

            try
            {
                using var client = new SmtpClient();

                bool usarSsl = bool.Parse(_configuration["Smtp:UsarSsl"] ?? "true");
                int porta = int.Parse(_configuration["Smtp:Port"] ?? "587");

                await client.ConnectAsync(
                    _configuration["Smtp:Host"],
                    porta,
                    usarSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None);

                await client.AuthenticateAsync(
                    _configuration["Smtp:Usuario"],
                    _configuration["Smtp:Senha"]);

                await client.SendAsync(mensagem);
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao enviar e-mail de redefinicao de senha para {Email}", destinatario);
                throw new InvalidOperationException("Não foi possível enviar o e-mail de redefinição no momento.");
            }
        }
    }
}