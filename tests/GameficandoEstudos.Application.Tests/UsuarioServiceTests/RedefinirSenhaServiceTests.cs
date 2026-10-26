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
    public class RedefinirSenhaServiceTests
    {
        private readonly Mock<IUsuarioRepository> _usuarioRepoMock = new();
        private readonly Mock<IPasswordService> _passwordServiceMock = new();
        private readonly Mock<IRefreshTokenRepository> _refreshRepoMock = new();

        private RedefinirSenhaService CriarService() => new(
            _usuarioRepoMock.Object,
            _passwordServiceMock.Object,
            new PoliticaSenhaValidator(),
            _refreshRepoMock.Object);

        private void NaoDeveTerRevogadoSessoes() =>
            _refreshRepoMock.Verify(r => r.RevogarTodosPorUsuarioAsync(It.IsAny<int>()), Times.Never);

        [Fact]
        public async Task ExecutarAsync_QuandoNovaSenhaNaoAtendePolitica_DeveLancarSemConsultarRepositorio()
        {
            var dto = new RedefinirSenhaDto("usuario@email.com", "token_qualquer", "senhafraca");

            Func<Task> act = async () => await CriarService().ExecutarAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A senha não atende à política de segurança.");

            _usuarioRepoMock.Verify(repo => repo.ObterPorEmailAsync(It.IsAny<string>()), Times.Never);
            NaoDeveTerRevogadoSessoes();
        }

        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioNaoExiste_DeveLancarComMensagemGenerica()
        {
            var dto = new RedefinirSenhaDto("naoexiste@email.com", "token_qualquer", "SenhaAbc123");
            _usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email)).ReturnsAsync((Usuario?)null);

            Func<Task> act = async () => await CriarService().ExecutarAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Token inválido ou expirado.");

            NaoDeveTerRevogadoSessoes();
        }

        [Fact]
        public async Task ExecutarAsync_QuandoTokenInvalidoOuExpirado_DeveLancarComMesmaMensagemGenerica()
        {
            var dto = new RedefinirSenhaDto("usuario@email.com", "token_errado", "SenhaAbc123");
            var usuarioFake = new Usuario("Usuario Teste", dto.Email, "hash_senha_antiga", TipoUsuario.Aluno);
            usuarioFake.DefinirTokenRedefinicaoSenha(
                TokenHashHelper.GerarHash("token_correto"), DateTime.UtcNow.AddHours(1));

            _usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email)).ReturnsAsync(usuarioFake);

            Func<Task> act = async () => await CriarService().ExecutarAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Token inválido ou expirado.");

            _passwordServiceMock.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
            NaoDeveTerRevogadoSessoes();
        }

        [Fact]
        public async Task ExecutarAsync_QuandoTokenValido_DeveAtualizarSenhaLimparTokenERevogarSessoes()
        {
            const string tokenBruto = "token_correto";
            var dto = new RedefinirSenhaDto("usuario@email.com", tokenBruto, "SenhaNovaAbc1");
            var usuarioFake = new Usuario("Usuario Teste", dto.Email, "hash_senha_antiga", TipoUsuario.Aluno);
            usuarioFake.DefinirTokenRedefinicaoSenha(
                TokenHashHelper.GerarHash(tokenBruto), DateTime.UtcNow.AddHours(1));

            _usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email)).ReturnsAsync(usuarioFake);
            _passwordServiceMock.Setup(p => p.HashPassword(dto.NovaSenha)).Returns("$argon2id$v=19$hash_novo");

            await CriarService().ExecutarAsync(dto);

            usuarioFake.Senha.Should().Be("$argon2id$v=19$hash_novo");
            usuarioFake.TokenRedefinicaoSenhaHash.Should().BeNull();
            usuarioFake.TokenRedefinicaoSenhaExpiraEm.Should().BeNull();

            _usuarioRepoMock.Verify(repo => repo.AtualizarAsync(usuarioFake), Times.Once);
            _refreshRepoMock.Verify(r => r.RevogarTodosPorUsuarioAsync(usuarioFake.Id), Times.Once);
        }
    }
}