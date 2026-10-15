
using FluentAssertions;
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
    public class RefreshTokenServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoTokenNaoEncontrado_DeveLancarUnauthorized()
        {
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var tokenServiceMock = new Mock<ITokenService>();

            refreshTokenRepoMock.Setup(r => r.ObterPorHashAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);

            var service = new RefreshTokenService(refreshTokenRepoMock.Object, usuarioRepoMock.Object, tokenServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync("token-inexistente");

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Sessão expirada. Faça login novamente.");

            usuarioRepoMock.Verify(r => r.ObterPorIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoTokenJaRevogado_DeveLancarUnauthorized()
        {
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var tokenServiceMock = new Mock<ITokenService>();

            const string tokenBruto = "token-revogado";
            var hash = TokenHashHelper.GerarHash(tokenBruto);
            var refreshToken = new RefreshToken(hash, idUsuario: 5, DateTime.UtcNow.AddDays(5));
            refreshToken.Revogar();

            refreshTokenRepoMock.Setup(r => r.ObterPorHashAsync(hash)).ReturnsAsync(refreshToken);

            var service = new RefreshTokenService(refreshTokenRepoMock.Object, usuarioRepoMock.Object, tokenServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(tokenBruto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Sessão expirada. Faça login novamente.");

            usuarioRepoMock.Verify(r => r.ObterPorIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoTokenExpirado_DeveLancarUnauthorized()
        {
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var tokenServiceMock = new Mock<ITokenService>();

            const string tokenBruto = "token-expirado";
            var hash = TokenHashHelper.GerarHash(tokenBruto);
            var refreshToken = new RefreshToken(hash, idUsuario: 5, DateTime.UtcNow.AddDays(-1));

            refreshTokenRepoMock.Setup(r => r.ObterPorHashAsync(hash)).ReturnsAsync(refreshToken);

            var service = new RefreshTokenService(refreshTokenRepoMock.Object, usuarioRepoMock.Object, tokenServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(tokenBruto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Sessão expirada. Faça login novamente.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioNaoExisteMais_DeveLancarSemRevogarToken()
        {
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var tokenServiceMock = new Mock<ITokenService>();

            const string tokenBruto = "token-usuario-sumiu";
            var hash = TokenHashHelper.GerarHash(tokenBruto);
            var refreshToken = new RefreshToken(hash, idUsuario: 999, DateTime.UtcNow.AddDays(5));

            refreshTokenRepoMock.Setup(r => r.ObterPorHashAsync(hash)).ReturnsAsync(refreshToken);
            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(999)).ReturnsAsync((Usuario?)null);

            var service = new RefreshTokenService(refreshTokenRepoMock.Object, usuarioRepoMock.Object, tokenServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(tokenBruto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Sessão expirada. Faça login novamente.");

            refreshTokenRepoMock.Verify(r => r.AtualizarAsync(It.IsAny<RefreshToken>()), Times.Never);
            refreshToken.RevogadoEm.Should().BeNull();
        }

        [Fact]
        public async Task ExecutarAsync_QuandoTokenValido_DeveRotacionarERetornarNovoPar()
        {
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var tokenServiceMock = new Mock<ITokenService>();

            const string tokenBrutoOriginal = "token-bruto-original";
            var hashOriginal = TokenHashHelper.GerarHash(tokenBrutoOriginal);
            var refreshTokenExistente = new RefreshToken(hashOriginal, idUsuario: 5, DateTime.UtcNow.AddDays(5));

            var usuario = new Usuario("Aluno Teste", "aluno@teste.com", "hash_senha", TipoUsuario.Aluno);
            typeof(Usuario).GetProperty("Id")?.SetValue(usuario, 5);

            refreshTokenRepoMock.Setup(r => r.ObterPorHashAsync(hashOriginal)).ReturnsAsync(refreshTokenExistente);
            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(usuario);
            tokenServiceMock.Setup(t => t.ObterDuracaoRefreshTokenEmDias()).Returns(7);
            tokenServiceMock.Setup(t => t.GerarToken(usuario)).Returns("novo-access-token");

            var service = new RefreshTokenService(refreshTokenRepoMock.Object, usuarioRepoMock.Object, tokenServiceMock.Object);

            var resultado = await service.ExecutarAsync(tokenBrutoOriginal);

            resultado.Token.Should().Be("novo-access-token");
            resultado.RefreshToken.Should().NotBeNullOrEmpty();
            resultado.RefreshToken.Should().NotBe(tokenBrutoOriginal);
            resultado.Id.Should().Be(5);

            refreshTokenExistente.RevogadoEm.Should().NotBeNull();
            refreshTokenExistente.SubstituidoPorHash.Should().NotBeNullOrEmpty();

            refreshTokenRepoMock.Verify(r => r.AtualizarAsync(refreshTokenExistente), Times.Once);
            refreshTokenRepoMock.Verify(r => r.AdicionarAsync(It.Is<RefreshToken>(rt => rt.IdUsuario == 5)), Times.Once);
        }
    }
}