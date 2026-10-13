using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Application.Validacoes;

namespace GameficandoEstudos.Application.Services.UsuarioService
{
    public class CadastrarUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordService _passwordService;
        private readonly PoliticaSenhaValidator _politicaSenhaValidator;

        public CadastrarUsuarioService(IUsuarioRepository usuarioRepository, IPasswordService passwordService, PoliticaSenhaValidator politicaSenhaValidator)
        {
            _usuarioRepository = usuarioRepository;
            _passwordService = passwordService;
            _politicaSenhaValidator = politicaSenhaValidator;
        }

        public async Task<int> ExecutarAsync(RegistrarUsuarioDto dto)
        {
            if (!_politicaSenhaValidator.EhValida(dto.Senha))
                throw new InvalidOperationException("A senha não atende à política de segurança.");
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