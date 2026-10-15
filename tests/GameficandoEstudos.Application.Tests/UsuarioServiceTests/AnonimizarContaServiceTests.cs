using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.UsuarioService;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.UsuarioServiceTests
{
    public class AnonimizarContaServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioNaoExiste_DeveLancarUnauthorized()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync((Usuario?)null);

            var service = new AnonimizarContaService(
                usuarioRepoMock.Object, passwordServiceMock.Object, refreshTokenRepoMock.Object, auditoriaServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, "senhaQualquer");

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Usuário não encontrado.");

            refreshTokenRepoMock.Verify(r => r.RevogarTodosPorUsuarioAsync(It.IsAny<int>()), Times.Never);
            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoSenhaIncorreta_DeveLancarUnauthorizedSemAnonimizar()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            var usuario = new Usuario("Aluno Teste", "aluno@teste.com", "hash_senha", TipoUsuario.Aluno);
            typeof(Usuario).GetProperty("Id")?.SetValue(usuario, 1);

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(usuario);
            passwordServiceMock.Setup(p => p.VerifyPassword(usuario.Senha, "senhaErrada")).Returns(false);

            var service = new AnonimizarContaService(
                usuarioRepoMock.Object, passwordServiceMock.Object, refreshTokenRepoMock.Object, auditoriaServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, "senhaErrada");

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Senha incorreta.");

            usuario.AnonimizadoEm.Should().BeNull();
            usuario.Nome.Should().Be("Aluno Teste");
            usuarioRepoMock.Verify(r => r.AtualizarAsync(It.IsAny<Usuario>()), Times.Never);
            refreshTokenRepoMock.Verify(r => r.RevogarTodosPorUsuarioAsync(It.IsAny<int>()), Times.Never);
            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoSenhaCorreta_DeveAnonimizarRevogarTokensERegistrarAuditoria()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            var usuario = new Usuario("Aluno Teste", "aluno@teste.com", "hash_senha", TipoUsuario.Aluno);
            typeof(Usuario).GetProperty("Id")?.SetValue(usuario, 1);
            var emailOriginal = usuario.Email;

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(usuario);
            passwordServiceMock.Setup(p => p.VerifyPassword(usuario.Senha, "senhaCorreta")).Returns(true);
            passwordServiceMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("$argon2id$hash_inutilizavel");

            var service = new AnonimizarContaService(
                usuarioRepoMock.Object, passwordServiceMock.Object, refreshTokenRepoMock.Object, auditoriaServiceMock.Object);

            await service.ExecutarAsync(1, "senhaCorreta");

            usuario.AnonimizadoEm.Should().NotBeNull();
            usuario.Nome.Should().Be("Usuário Removido");
            usuario.Email.Should().NotBe(emailOriginal);
            usuario.Email.Should().EndWith("@anonimizado.local");
            usuario.Senha.Should().Be("$argon2id$hash_inutilizavel");
            usuario.TokenRedefinicaoSenhaHash.Should().BeNull();

            usuarioRepoMock.Verify(r => r.AtualizarAsync(usuario), Times.Once);
            refreshTokenRepoMock.Verify(r => r.RevogarTodosPorUsuarioAsync(1), Times.Once);
            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                1, "Usuario.Anonimizar", "Usuario", "1", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoContaJaAnonimizada_DeveSerIdempotente()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var refreshTokenRepoMock = new Mock<IRefreshTokenRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            var usuario = new Usuario("Aluno Teste", "aluno@teste.com", "hash_senha", TipoUsuario.Aluno);
            typeof(Usuario).GetProperty("Id")?.SetValue(usuario, 1);

            passwordServiceMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("$argon2id$hash_1");
            usuario.Anonimizar("$argon2id$hash_1"); 
            var emailAposPrimeiraAnonimizacao = usuario.Email;
            var anonimizadoEmOriginal = usuario.AnonimizadoEm;

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(usuario);
            passwordServiceMock.Setup(p => p.VerifyPassword(usuario.Senha, "qualquerSenha")).Returns(true);
            passwordServiceMock.Setup(p => p.HashPassword(It.IsAny<string>())).Returns("$argon2id$hash_2");

            var service = new AnonimizarContaService(
                usuarioRepoMock.Object, passwordServiceMock.Object, refreshTokenRepoMock.Object, auditoriaServiceMock.Object);

            await service.ExecutarAsync(1, "qualquerSenha");

            usuario.Email.Should().Be(emailAposPrimeiraAnonimizacao);
            usuario.AnonimizadoEm.Should().Be(anonimizadoEmOriginal);
        }
    }
}