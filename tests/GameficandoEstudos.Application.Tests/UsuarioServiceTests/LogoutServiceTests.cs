
using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.UsuarioService;
using GameficandoEstudos.Application.Validacoes;
using GameficandoEstudos.Domain.Entities;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.UsuarioServiceTests
{
    public class LogoutServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoTokenExiste_DeveRevogar()
        {
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

            const string tokenBruto = "token-para-logout";
            var hash = TokenHashHelper.GerarHash(tokenBruto);
            var refreshToken = new RefreshToken(hash, idUsuario: 5, DateTime.UtcNow.AddDays(5));

            refreshTokenRepoMock.Setup(r => r.ObterPorHashAsync(hash)).ReturnsAsync(refreshToken);

            var service = new LogoutService(refreshTokenRepoMock.Object);

            await service.ExecutarAsync(tokenBruto);

            refreshToken.RevogadoEm.Should().NotBeNull();
            refreshTokenRepoMock.Verify(r => r.AtualizarAsync(refreshToken), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoTokenNaoExiste_NaoDeveLancarNemChamarAtualizar()
        {
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

            refreshTokenRepoMock.Setup(r => r.ObterPorHashAsync(It.IsAny<string>())).ReturnsAsync((RefreshToken?)null);

            var service = new LogoutService(refreshTokenRepoMock.Object);

            await service.ExecutarAsync("token-inexistente");

            refreshTokenRepoMock.Verify(r => r.AtualizarAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoTokenJaRevogado_DeveSerIdempotente()
        {
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

            const string tokenBruto = "token-ja-deslogado";
            var hash = TokenHashHelper.GerarHash(tokenBruto);
            var refreshToken = new RefreshToken(hash, idUsuario: 5, DateTime.UtcNow.AddDays(5));
            refreshToken.Revogar();
            var revogadoEmOriginal = refreshToken.RevogadoEm;

            refreshTokenRepoMock.Setup(r => r.ObterPorHashAsync(hash)).ReturnsAsync(refreshToken);

            var service = new LogoutService(refreshTokenRepoMock.Object);

            await service.ExecutarAsync(tokenBruto);

            refreshToken.RevogadoEm.Should().Be(revogadoEmOriginal); 
            refreshTokenRepoMock.Verify(r => r.AtualizarAsync(It.IsAny<RefreshToken>()), Times.Never);
        }
    }
}