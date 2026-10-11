using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services
{
    public class AutenticarUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordService _passwordService;
        private readonly ITokenService _tokenService;

        public AutenticarUsuarioService(IUsuarioRepository usuarioRepository, IPasswordService passwordService, ITokenService tokenService)
        {
            _usuarioRepository = usuarioRepository;
            _passwordService = passwordService;
            _tokenService = tokenService;
        }

        public async Task<AuthResultDto> ExecutarAsync(LoginDto dto)
        {
            var usuario = await _usuarioRepository.ObterPorEmailAsync(dto.Email);
            if (usuario == null)
                throw new UnauthorizedAccessException("E-mail ou senha incorretos.");

            bool senhaValida = _passwordService.VerifyPassword(usuario.Senha, dto.Senha);
            if (!senhaValida)
                throw new UnauthorizedAccessException("E-mail ou senha incorretos.");

            string token = _tokenService.GerarToken(usuario);

            return new AuthResultDto(usuario.Id, usuario.Nome, token);
        }
    }
}