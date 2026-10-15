using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Interfaces.Security;
using GameficandoEstudos.Application.Services.UsuarioService;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.UsuarioServiceTests
{
    public class AutenticarUsuarioServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoEmailEstaBloqueado_DeveLancarExcecaoSemConsultarRepositorio()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var tokenServiceMock = new Mock<ITokenService>();
            var loginAttemptGuardMock = new Mock<ILoginAttemptGuard>();
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

            var dto = new LoginDto("bloqueado@email.com", "senha123");

            loginAttemptGuardMock.Setup(g => g.EstaBloqueado(dto.Email)).Returns(true);

            var service = new AutenticarUsuarioService(
                usuarioRepoMock.Object,
                passwordServiceMock.Object,
                tokenServiceMock.Object,
                loginAttemptGuardMock.Object,
                refreshTokenRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("E-mail ou senha incorretos.");

            usuarioRepoMock.Verify(repo => repo.ObterPorEmailAsync(It.IsAny<string>()), Times.Never);
            loginAttemptGuardMock.Verify(g => g.RegistrarFalha(It.IsAny<string>()), Times.Never);
            refreshTokenRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioNaoExiste_DeveLancarExcecaoERegistrarFalha()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var tokenServiceMock = new Mock<ITokenService>();
            var loginAttemptGuardMock = new Mock<ILoginAttemptGuard>();
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

            var dto = new LoginDto("inexistente@email.com", "senha123");

            loginAttemptGuardMock.Setup(g => g.EstaBloqueado(dto.Email)).Returns(false);
            usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email))
                           .ReturnsAsync((Usuario?)null);

            var service = new AutenticarUsuarioService(
                usuarioRepoMock.Object,
                passwordServiceMock.Object,
                tokenServiceMock.Object,
                loginAttemptGuardMock.Object,
                refreshTokenRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("E-mail ou senha incorretos.");

            loginAttemptGuardMock.Verify(g => g.RegistrarFalha(dto.Email), Times.Once);
            refreshTokenRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoSenhaIncorreta_DeveLancarExcecaoERegistrarFalha()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var tokenServiceMock = new Mock<ITokenService>();
            var loginAttemptGuardMock = new Mock<ILoginAttemptGuard>();
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

            var dto = new LoginDto("usuario@email.com", "senha_errada");
            var usuarioFake = new Usuario("Nome", dto.Email, "hash_senha_correta", TipoUsuario.Professor);

            loginAttemptGuardMock.Setup(g => g.EstaBloqueado(dto.Email)).Returns(false);
            usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email)).ReturnsAsync(usuarioFake);
            passwordServiceMock.Setup(p => p.VerifyPassword(usuarioFake.Senha, dto.Senha)).Returns(false);

            var service = new AutenticarUsuarioService(
                usuarioRepoMock.Object,
                passwordServiceMock.Object,
                tokenServiceMock.Object,
                loginAttemptGuardMock.Object,
                refreshTokenRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("E-mail ou senha incorretos.");

            tokenServiceMock.Verify(t => t.GerarToken(It.IsAny<Usuario>()), Times.Never);
            loginAttemptGuardMock.Verify(g => g.RegistrarFalha(dto.Email), Times.Once);
            refreshTokenRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<RefreshToken>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoCredenciaisValidas_DeveRetornarAuthResultComTokensERegistrarSucesso()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var tokenServiceMock = new Mock<ITokenService>();
            var loginAttemptGuardMock = new Mock<ILoginAttemptGuard>();
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();

            var dto = new LoginDto("valido@email.com", "senha123");
            var usuarioFake = new Usuario("Professor Valido", dto.Email, "hash_senha", TipoUsuario.Professor);
            typeof(Usuario).GetProperty("Id")?.SetValue(usuarioFake, 42);

            string tokenFake = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.token_fake";

            loginAttemptGuardMock.Setup(g => g.EstaBloqueado(dto.Email)).Returns(false);
            usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email)).ReturnsAsync(usuarioFake);
            passwordServiceMock.Setup(p => p.VerifyPassword(usuarioFake.Senha, dto.Senha)).Returns(true);
            tokenServiceMock.Setup(t => t.GerarToken(usuarioFake)).Returns(tokenFake);
            tokenServiceMock.Setup(t => t.ObterDuracaoRefreshTokenEmDias()).Returns(7);

            var service = new AutenticarUsuarioService(
                usuarioRepoMock.Object,
                passwordServiceMock.Object,
                tokenServiceMock.Object,
                loginAttemptGuardMock.Object,
                refreshTokenRepoMock.Object);

            var resultado = await service.ExecutarAsync(dto);

            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(42);
            resultado.Nome.Should().Be("Professor Valido");
            resultado.Token.Should().Be(tokenFake);
            resultado.RefreshToken.Should().NotBeNullOrEmpty();
            resultado.TipoUsuario.Should().Be(TipoUsuario.Professor);

            tokenServiceMock.Verify(t => t.GerarToken(usuarioFake), Times.Once);
            loginAttemptGuardMock.Verify(g => g.RegistrarSucesso(dto.Email), Times.Once);
            refreshTokenRepoMock.Verify(r => r.AdicionarAsync(It.Is<RefreshToken>(
                rt => rt.IdUsuario == 42)), Times.Once);
        }
    }
}