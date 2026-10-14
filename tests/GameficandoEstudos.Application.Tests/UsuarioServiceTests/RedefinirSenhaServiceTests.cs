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
        [Fact]
        public async Task ExecutarAsync_QuandoNovaSenhaNaoAtendePolitica_DeveLancarSemConsultarRepositorio()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var politicaSenhaValidator = new PoliticaSenhaValidator();

            var dto = new RedefinirSenhaDto("usuario@email.com", "token_qualquer", "senhafraca");

            var service = new RedefinirSenhaService(
                usuarioRepoMock.Object, passwordServiceMock.Object, politicaSenhaValidator);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("A senha não atende à política de segurança.");

            usuarioRepoMock.Verify(repo => repo.ObterPorEmailAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioNaoExiste_DeveLancarComMensagemGenerica()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var politicaSenhaValidator = new PoliticaSenhaValidator();

            var dto = new RedefinirSenhaDto("naoexiste@email.com", "token_qualquer", "SenhaAbc123");

            usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email)).ReturnsAsync((Usuario?)null);

            var service = new RedefinirSenhaService(
                usuarioRepoMock.Object, passwordServiceMock.Object, politicaSenhaValidator);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Token inválido ou expirado.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoTokenInvalidoOuExpirado_DeveLancarComMesmaMensagemGenerica()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var politicaSenhaValidator = new PoliticaSenhaValidator();

            var dto = new RedefinirSenhaDto("usuario@email.com", "token_errado", "SenhaAbc123");
            var usuarioFake = new Usuario("Usuario Teste", dto.Email, "hash_senha_antiga", TipoUsuario.Aluno);
            usuarioFake.DefinirTokenRedefinicaoSenha(
                TokenHashHelper.GerarHash("token_correto"), DateTime.UtcNow.AddHours(1));

            usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email)).ReturnsAsync(usuarioFake);

            var service = new RedefinirSenhaService(
                usuarioRepoMock.Object, passwordServiceMock.Object, politicaSenhaValidator);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Token inválido ou expirado.");

            passwordServiceMock.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoTokenValido_DeveAtualizarSenhaELimparToken()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var politicaSenhaValidator = new PoliticaSenhaValidator();

            const string tokenBruto = "token_correto";
            var dto = new RedefinirSenhaDto("usuario@email.com", tokenBruto, "SenhaNovaAbc1");
            var usuarioFake = new Usuario("Usuario Teste", dto.Email, "hash_senha_antiga", TipoUsuario.Aluno);
            usuarioFake.DefinirTokenRedefinicaoSenha(
                TokenHashHelper.GerarHash(tokenBruto), DateTime.UtcNow.AddHours(1));

            usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email)).ReturnsAsync(usuarioFake);
            passwordServiceMock.Setup(p => p.HashPassword(dto.NovaSenha)).Returns("$argon2id$v=19$hash_novo");

            var service = new RedefinirSenhaService(
                usuarioRepoMock.Object, passwordServiceMock.Object, politicaSenhaValidator);

            await service.ExecutarAsync(dto);

            usuarioFake.Senha.Should().Be("$argon2id$v=19$hash_novo");
            usuarioFake.TokenRedefinicaoSenhaHash.Should().BeNull();
            usuarioFake.TokenRedefinicaoSenhaExpiraEm.Should().BeNull();

            usuarioRepoMock.Verify(repo => repo.AtualizarAsync(usuarioFake), Times.Once);
        }
    }
}