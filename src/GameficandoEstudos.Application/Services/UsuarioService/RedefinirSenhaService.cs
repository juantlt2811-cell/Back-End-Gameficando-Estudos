using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Validacoes;

namespace GameficandoEstudos.Application.Services.UsuarioService
{
    public class RedefinirSenhaService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordService _passwordService;
        private readonly PoliticaSenhaValidator _politicaSenhaValidator;

        public RedefinirSenhaService(
            IUsuarioRepository usuarioRepository,
            IPasswordService passwordService,
            PoliticaSenhaValidator politicaSenhaValidator)
        {
            _usuarioRepository = usuarioRepository;
            _passwordService = passwordService;
            _politicaSenhaValidator = politicaSenhaValidator;
        }

        public async Task ExecutarAsync(RedefinirSenhaDto dto)
        {
            if (!_politicaSenhaValidator.EhValida(dto.NovaSenha))
                throw new InvalidOperationException("A senha não atende à política de segurança.");

            var usuario = await _usuarioRepository.ObterPorEmailAsync(dto.Email);
            if (usuario == null)
                throw new InvalidOperationException("Token inválido ou expirado.");

            var tokenHash = TokenHashHelper.GerarHash(dto.Token);
            if (!usuario.TokenRedefinicaoSenhaEhValido(tokenHash))
                throw new InvalidOperationException("Token inválido ou expirado.");

            var novaSenhaHash = _passwordService.HashPassword(dto.NovaSenha);
            usuario.AtualizarSenha(novaSenhaHash);
            usuario.LimparTokenRedefinicaoSenha();

            await _usuarioRepository.AtualizarAsync(usuario);
        }
    }
}