using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Validacoes;

namespace GameficandoEstudos.Application.Services.UsuarioService
{
    public class AlterarSenhaService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordService _passwordService;
        private readonly PoliticaSenhaValidator _politicaSenhaValidator;

        public AlterarSenhaService(
            IUsuarioRepository usuarioRepository,
            IPasswordService passwordService,
            PoliticaSenhaValidator politicaSenhaValidator)
        {
            _usuarioRepository = usuarioRepository;
            _passwordService = passwordService;
            _politicaSenhaValidator = politicaSenhaValidator;
        }

        public async Task ExecutarAsync(int idUsuarioAutenticado, AlterarSenhaDto dto)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(idUsuarioAutenticado);
            if (usuario == null)
                throw new UnauthorizedAccessException("Usuário não encontrado.");

            if (!_passwordService.VerifyPassword(usuario.Senha, dto.SenhaAtual))
                throw new UnauthorizedAccessException("Senha atual incorreta.");

            if (!_politicaSenhaValidator.EhValida(dto.NovaSenha))
                throw new InvalidOperationException("A nova senha não atende à política de segurança.");

            var novaSenhaHash = _passwordService.HashPassword(dto.NovaSenha);
            usuario.AtualizarSenha(novaSenhaHash);

            await _usuarioRepository.AtualizarAsync(usuario);
        }
    }
}