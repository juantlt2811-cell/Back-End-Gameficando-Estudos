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
        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioNaoExiste_DeveLancarUnauthorized()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var politicaSenhaValidator = new PoliticaSenhaValidator();

            var dto = new AlterarSenhaDto("senhaAtual123", "SenhaNovaAbc1");

            usuarioRepoMock.Setup(repo => repo.ObterPorIdAsync(1)).ReturnsAsync((Usuario?)null);

            var service = new AlterarSenhaService(
                usuarioRepoMock.Object, passwordServiceMock.Object, politicaSenhaValidator);

            Func<Task> act = async () => await service.ExecutarAsync(1, dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Usuário não encontrado.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoSenhaAtualIncorreta_DeveLancarUnauthorized()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var politicaSenhaValidator = new PoliticaSenhaValidator();

            var dto = new AlterarSenhaDto("senhaErrada", "SenhaNovaAbc1");
            var usuarioFake = new Usuario("Usuario Teste", "usuario@email.com", "hash_senha_atual", TipoUsuario.Aluno);

            usuarioRepoMock.Setup(repo => repo.ObterPorIdAsync(1)).ReturnsAsync(usuarioFake);
            passwordServiceMock.Setup(p => p.VerifyPassword(usuarioFake.Senha, dto.SenhaAtual)).Returns(false);

            var service = new AlterarSenhaService(
                usuarioRepoMock.Object, passwordServiceMock.Object, politicaSenhaValidator);

            Func<Task> act = async () => await service.ExecutarAsync(1, dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Senha atual incorreta.");

            passwordServiceMock.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoNovaSenhaNaoAtendePolitica_DeveLancarInvalidOperation()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var politicaSenhaValidator = new PoliticaSenhaValidator();

            var dto = new AlterarSenhaDto("senhaAtual123", "senhafraca");
            var usuarioFake = new Usuario("Usuario Teste", "usuario@email.com", "hash_senha_atual", TipoUsuario.Aluno);

            usuarioRepoMock.Setup(repo => repo.ObterPorIdAsync(1)).ReturnsAsync(usuarioFake);
            passwordServiceMock.Setup(p => p.VerifyPassword(usuarioFake.Senha, dto.SenhaAtual)).Returns(true);

            var service = new AlterarSenhaService(
                usuarioRepoMock.Object, passwordServiceMock.Object, politicaSenhaValidator);

            Func<Task> act = async () => await service.ExecutarAsync(1, dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A nova senha não atende à política de segurança.");

            usuarioRepoMock.Verify(repo => repo.AtualizarAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoDadosValidos_DeveAtualizarSenha()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var politicaSenhaValidator = new PoliticaSenhaValidator();

            var dto = new AlterarSenhaDto("senhaAtual123", "SenhaNovaAbc1");
            var usuarioFake = new Usuario("Usuario Teste", "usuario@email.com", "hash_senha_atual", TipoUsuario.Aluno);

            usuarioRepoMock.Setup(repo => repo.ObterPorIdAsync(1)).ReturnsAsync(usuarioFake);
            passwordServiceMock.Setup(p => p.VerifyPassword(usuarioFake.Senha, dto.SenhaAtual)).Returns(true);
            passwordServiceMock.Setup(p => p.HashPassword(dto.NovaSenha)).Returns("$argon2id$v=19$hash_novo");

            var service = new AlterarSenhaService(
                usuarioRepoMock.Object, passwordServiceMock.Object, politicaSenhaValidator);

            await service.ExecutarAsync(1, dto);

            usuarioFake.Senha.Should().Be("$argon2id$v=19$hash_novo");
            usuarioRepoMock.Verify(repo => repo.AtualizarAsync(usuarioFake), Times.Once);
        }
    }
}