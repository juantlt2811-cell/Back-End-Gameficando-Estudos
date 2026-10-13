using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Validacoes;

namespace GameficandoEstudos.Application.Services.UsuarioService
{
    public class SolicitarRedefinicaoSenhaService
    {
        private static readonly TimeSpan ValidadeToken = TimeSpan.FromHours(1);

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IEmailService _emailService;

        public SolicitarRedefinicaoSenhaService(IUsuarioRepository usuarioRepository, IEmailService emailService)
        {
            _usuarioRepository = usuarioRepository;
            _emailService = emailService;
        }

        public async Task ExecutarAsync(SolicitarRedefinicaoSenhaDto dto)
        {
            var usuario = await _usuarioRepository.ObterPorEmailAsync(dto.Email);

            if (usuario == null)
                return;

            var tokenBruto = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            var tokenHash = TokenHashHelper.GerarHash(tokenBruto);

            usuario.DefinirTokenRedefinicaoSenha(tokenHash, DateTime.UtcNow.Add(ValidadeToken));
            await _usuarioRepository.AtualizarAsync(usuario);

            await _emailService.EnviarEmailRedefinicaoSenhaAsync(usuario.Email, usuario.Nome, tokenBruto);
        }
    }
}