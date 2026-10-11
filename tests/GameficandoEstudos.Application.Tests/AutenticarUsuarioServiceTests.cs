using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests
{
    public class AutenticarUsuarioServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioNaoExiste_DeveLancarExcecao()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var tokenServiceMock = new Mock<ITokenService>();

            var dto = new LoginDto("inexistente@email.com", "senha123");

            usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email))
                           .ReturnsAsync((Usuario?)null);

            var service = new AutenticarUsuarioService(usuarioRepoMock.Object, passwordServiceMock.Object, tokenServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("E-mail ou senha incorretos.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoSenhaIncorreta_DeveLancarExcecao()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var tokenServiceMock = new Mock<ITokenService>();

            var dto = new LoginDto("usuario@email.com", "senha_errada");
            var usuarioFake = new Usuario("Nome", dto.Email, "hash_senha_correta", TipoUsuario.Professor);

            usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email)).ReturnsAsync(usuarioFake);
            passwordServiceMock.Setup(p => p.VerifyPassword(usuarioFake.Senha, dto.Senha)).Returns(false);

            var service = new AutenticarUsuarioService(usuarioRepoMock.Object, passwordServiceMock.Object, tokenServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("E-mail ou senha incorretos.");

            tokenServiceMock.Verify(t => t.GerarToken(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoCredenciaisValidas_DeveRetornarAuthResultComToken()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var tokenServiceMock = new Mock<ITokenService>();

            var dto = new LoginDto("valido@email.com", "senha123");
            var usuarioFake = new Usuario("Professor Valido", dto.Email, "hash_senha", TipoUsuario.Professor);
            typeof(Usuario).GetProperty("Id")?.SetValue(usuarioFake, 42);

            string tokenFake = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.token_fake";

            usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email)).ReturnsAsync(usuarioFake);
            passwordServiceMock.Setup(p => p.VerifyPassword(usuarioFake.Senha, dto.Senha)).Returns(true);
            tokenServiceMock.Setup(t => t.GerarToken(usuarioFake)).Returns(tokenFake);

            var service = new AutenticarUsuarioService(usuarioRepoMock.Object, passwordServiceMock.Object, tokenServiceMock.Object);

            var resultado = await service.ExecutarAsync(dto);

            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(42);
            resultado.Nome.Should().Be("Professor Valido");
            resultado.Token.Should().Be(tokenFake);

            tokenServiceMock.Verify(t => t.GerarToken(usuarioFake), Times.Once);
        }
    }
}