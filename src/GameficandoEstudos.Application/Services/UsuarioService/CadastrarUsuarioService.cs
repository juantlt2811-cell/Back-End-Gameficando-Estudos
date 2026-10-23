using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using GameficandoEstudos.Application.Validacoes;

namespace GameficandoEstudos.Application.Services.UsuarioService
{
    public class CadastrarUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordService _passwordService;
        private readonly PoliticaSenhaValidator _politicaSenhaValidator;
        private readonly CadastroOptions _opcoes;

        public CadastrarUsuarioService(
            IUsuarioRepository usuarioRepository,
            IPasswordService passwordService,
            PoliticaSenhaValidator politicaSenhaValidator,
            CadastroOptions opcoes)
        {
            _usuarioRepository = usuarioRepository;
            _passwordService = passwordService;
            _politicaSenhaValidator = politicaSenhaValidator;
            _opcoes = opcoes;
        }

        public async Task<int> ExecutarAsync(RegistrarUsuarioDto dto)
        {
            if (!_politicaSenhaValidator.EhValida(dto.Senha))
                throw new InvalidOperationException("A senha não atende à política de segurança.");

            
            var tipo = DefinirTipoUsuario(dto.CodigoProfessor);

            var emailJaExiste = await _usuarioRepository.ExisteEmailAsync(dto.Email);
            if (emailJaExiste)
                throw new InvalidOperationException("Este e-mail já está em uso.");

            string senhaHash = _passwordService.HashPassword(dto.Senha);

            var usuario = new Usuario(dto.Nome, dto.Email, senhaHash, tipo);

            await _usuarioRepository.AdicionarAsync(usuario);

            return usuario.Id;
        }

        private TipoUsuario DefinirTipoUsuario(string? codigoInformado)
        {
            if (string.IsNullOrWhiteSpace(codigoInformado))
                return TipoUsuario.Aluno;

            
            if (string.IsNullOrWhiteSpace(_opcoes.CodigoProfessor) || !CodigosIguais(codigoInformado, _opcoes.CodigoProfessor))
                throw new InvalidOperationException("Código de professor inválido.");

            return TipoUsuario.Professor;
        }

        
        private static bool CodigosIguais(string informado, string esperado) =>
            CryptographicOperations.FixedTimeEquals(
                SHA256.HashData(Encoding.UTF8.GetBytes(informado)),
                SHA256.HashData(Encoding.UTF8.GetBytes(esperado)));
    }
}