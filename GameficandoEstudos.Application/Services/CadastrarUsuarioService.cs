using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Services
{
    public class CadastrarUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordService _passwordService;

        public CadastrarUsuarioService(IUsuarioRepository usuarioRepository, IPasswordService passwordService)
        {
            _usuarioRepository = usuarioRepository;
            _passwordService = passwordService;
        }

        public async Task<int> ExecutarAsync(RegistrarUsuarioDto dto)
        {
            var emailJaExiste = await _usuarioRepository.ExisteEmailAsync(dto.Email);
            if (emailJaExiste)
                throw new InvalidOperationException("Este e-mail já está em uso.");

            string senhaHash = _passwordService.HashPassword(dto.Senha);

            var usuario = new Usuario(dto.Nome, dto.Email, senhaHash, dto.TipoUsuario);

            await _usuarioRepository.AdicionarAsync(usuario);

            return usuario.Id;
        }
    }
}