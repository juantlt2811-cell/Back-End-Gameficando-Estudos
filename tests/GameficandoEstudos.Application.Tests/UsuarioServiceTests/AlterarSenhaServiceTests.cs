using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.UsuarioService;
using GameficandoEstudos.Application.Validacoes;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.UsuarioServiceTests
{
    public class AlterarSenhaServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
        private readonly Mock<IPasswordService> _passwordServiceMock = new();
        private readonly Mock<IRefreshTokenRepository> _refreshRepoMock = new();

        private AlterarSenhaService CriarService() => new(
            _usuarioRepoMock.Object,
            _passwordServiceMock.Object,
            new PoliticaSenhaValidator(),
            _refreshRepoMock.Object);

        private void NaoDeveTerRevogadoSessoes() =>
            _refreshRepoMock.Verify(r => r.RevogarTodosPorUsuarioAsync(It.IsAny<int>()), Times.Never);

        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioNaoExiste_DeveLancarUnauthorized()
        {
            var dto = new AlterarSenhaDto("senhaAtual123", "SenhaNovaAbc1");
            _usuarioRepoMock.Setup(repo => repo.ObterPorIdAsync(1)).ReturnsAsync((Usuario?)null);

            Func<Task> act = async () => await CriarService().ExecutarAsync(1, dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Usuário não encontrado.");

            NaoDeveTerRevogadoSessoes();
        }

        [Fact]
        public async Task ExecutarAsync_QuandoSenhaAtualIncorreta_DeveLancarUnauthorized()
        {
            var dto = new AlterarSenhaDto("senhaErrada", "SenhaNovaAbc1");
            var usuarioFake = new Usuario("Usuario Teste", "usuario@email.com", "hash_senha_atual", TipoUsuario.Aluno);

            _usuarioRepoMock.Setup(repo => repo.ObterPorIdAsync(1)).ReturnsAsync(usuarioFake);
            _passwordServiceMock.Setup(p => p.VerifyPassword(usuarioFake.Senha, dto.SenhaAtual)).Returns(false);

            Func<Task> act = async () => await CriarService().ExecutarAsync(1, dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Senha atual incorreta.");

            _passwordServiceMock.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
            NaoDeveTerRevogadoSessoes();
        }

        [Fact]
        public async Task ExecutarAsync_QuandoNovaSenhaNaoAtendePolitica_DeveLancarInvalidOperation()
        {
            var dto = new AlterarSenhaDto("senhaAtual123", "senhafraca");
            var usuarioFake = new Usuario("Usuario Teste", "usuario@email.com", "hash_senha_atual", TipoUsuario.Aluno);

            _usuarioRepoMock.Setup(repo => repo.ObterPorIdAsync(1)).ReturnsAsync(usuarioFake);
            _passwordServiceMock.Setup(p => p.VerifyPassword(usuarioFake.Senha, dto.SenhaAtual)).Returns(true);

            Func<Task> act = async () => await CriarService().ExecutarAsync(1, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A nova senha não atende à política de segurança.");

            _usuarioRepoMock.Verify(repo => repo.AtualizarAsync(It.IsAny<Usuario>()), Times.Never);
            NaoDeveTerRevogadoSessoes();
        }

        [Fact]
        public async Task ExecutarAsync_QuandoDadosValidos_DeveAtualizarSenhaERevogarTodasAsSessoes()
        {
            var dto = new AlterarSenhaDto("senhaAtual123", "SenhaNovaAbc1");
            var usuarioFake = new Usuario("Usuario Teste", "usuario@email.com", "hash_senha_atual", TipoUsuario.Aluno);

            _usuarioRepoMock.Setup(repo => repo.ObterPorIdAsync(1)).ReturnsAsync(usuarioFake);
            _passwordServiceMock.Setup(p => p.VerifyPassword(usuarioFake.Senha, dto.SenhaAtual)).Returns(true);
            _passwordServiceMock.Setup(p => p.HashPassword(dto.NovaSenha)).Returns("$argon2id$v=19$hash_novo");

            await CriarService().ExecutarAsync(1, dto);

            usuarioFake.Senha.Should().Be("$argon2id$v=19$hash_novo");
            _usuarioRepoMock.Verify(repo => repo.AtualizarAsync(usuarioFake), Times.Once);
            _refreshRepoMock.Verify(r => r.RevogarTodosPorUsuarioAsync(1), Times.Once);
        }
    }
}