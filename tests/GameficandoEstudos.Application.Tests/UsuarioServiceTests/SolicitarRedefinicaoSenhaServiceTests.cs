using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.UsuarioService;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.UsuarioServiceTests
{
    public class SolicitarRedefinicaoSenhaServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoEmailNaoExiste_NaoDeveLancarNemEnviarEmail()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var emailServiceMock = new Mock<IEmailService>();

            var dto = new SolicitarRedefinicaoSenhaDto("naoexiste@email.com");

            usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email))
                           .ReturnsAsync((Usuario?)null);

            var service = new SolicitarRedefinicaoSenhaService(usuarioRepoMock.Object, emailServiceMock.Object);

            // Nao deve lancar excecao: do ponto de vista do chamador, sempre "sucesso" (anti-enumeracao).
            await service.ExecutarAsync(dto);

            usuarioRepoMock.Verify(repo => repo.AtualizarAsync(It.IsAny<Usuario>()), Times.Never);
            emailServiceMock.Verify(
                e => e.EnviarEmailRedefinicaoSenhaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoEmailExiste_DeveGerarTokenSalvarEEnviarEmail()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var emailServiceMock = new Mock<IEmailService>();

            var dto = new SolicitarRedefinicaoSenhaDto("usuario@email.com");
            var usuarioFake = new Usuario("Usuario Teste", dto.Email, "hash_senha", TipoUsuario.Aluno);

            usuarioRepoMock.Setup(repo => repo.ObterPorEmailAsync(dto.Email)).ReturnsAsync(usuarioFake);

            string? tokenEnviado = null;
            emailServiceMock
                .Setup(e => e.EnviarEmailRedefinicaoSenhaAsync(dto.Email, usuarioFake.Nome, It.IsAny<string>()))
                .Callback<string, string, string>((_, _, token) => tokenEnviado = token)
                .Returns(Task.CompletedTask);

            var service = new SolicitarRedefinicaoSenhaService(usuarioRepoMock.Object, emailServiceMock.Object);

            await service.ExecutarAsync(dto);

            usuarioFake.TokenRedefinicaoSenhaHash.Should().NotBeNullOrEmpty();
            usuarioFake.TokenRedefinicaoSenhaExpiraEm.Should().NotBeNull();
            usuarioFake.TokenRedefinicaoSenhaExpiraEm!.Value.Should()
                .BeCloseTo(DateTime.UtcNow.AddHours(1), TimeSpan.FromMinutes(1));

            tokenEnviado.Should().NotBeNullOrEmpty();
            tokenEnviado.Should().NotContain("+").And.NotContain("/").And.NotContain("="); // base64url

            usuarioRepoMock.Verify(repo => repo.AtualizarAsync(usuarioFake), Times.Once);
            emailServiceMock.Verify(
                e => e.EnviarEmailRedefinicaoSenhaAsync(dto.Email, usuarioFake.Nome, tokenEnviado!),
                Times.Once);
        }
    }
}