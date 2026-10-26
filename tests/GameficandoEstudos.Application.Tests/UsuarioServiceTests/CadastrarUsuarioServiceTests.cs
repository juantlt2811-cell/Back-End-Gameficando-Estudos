using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.UsuarioService;
using GameficandoEstudos.Application.Validacoes;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.UsuarioServiceTests
{
    public class CadastrarUsuarioServiceTests
    {
        private const string CodigoProfessorConfigurado = "codigo-secreto-de-teste-9f3a";

        private static (CadastrarUsuarioService Service, Mock<IUsuarioRepository> Repo, Mock<IPasswordService> Senha)
            CriarService(string? codigoProfessor = CodigoProfessorConfigurado)
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();

            usuarioRepoMock
                .Setup(repo => repo.ExisteEmailAsync(It.IsAny<string>()))
                .ReturnsAsync(false);

            passwordServiceMock
                .Setup(p => p.HashPassword(It.IsAny<string>()))
                .Returns("$argon2id$v=19$hash_fake");

            var service = new CadastrarUsuarioService(
                usuarioRepoMock.Object,
                passwordServiceMock.Object,
                new PoliticaSenhaValidator(),
                new CadastroOptions(codigoProfessor));

            return (service, usuarioRepoMock, passwordServiceMock);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoEmailJaExiste_DeveLancarExcecao()
        {
            var (service, usuarioRepoMock, passwordServiceMock) = CriarService();

            var dto = new RegistrarUsuarioDto("Professor Teste", "professor@email.com", "SenhaAbc123");

            usuarioRepoMock
                .Setup(repo => repo.ExisteEmailAsync(dto.Email))
                .ReturnsAsync(true);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Este e-mail já está em uso.");

            usuarioRepoMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Usuario>()), Times.Never);
            passwordServiceMock.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoDadosValidos_DeveCriptografarSenhaESalvar()
        {
            var (service, usuarioRepoMock, passwordServiceMock) = CriarService();

            var dto = new RegistrarUsuarioDto("Novo Aluno", "aluno@email.com", "SenhaAbc123");

            await service.ExecutarAsync(dto);

            passwordServiceMock.Verify(p => p.HashPassword(dto.Senha), Times.Once);

            usuarioRepoMock.Verify(repo => repo.AdicionarAsync(It.Is<Usuario>(u =>
                u.Nome == dto.Nome &&
                u.Email == dto.Email &&
                u.Senha == "$argon2id$v=19$hash_fake" &&
                u.Tipo == TipoUsuario.Aluno
            )), Times.Once);
        }

        [Theory]
        [InlineData("senhaabc123")]
        [InlineData("SENHAABC123")]
        [InlineData("SenhaAbcdef")]
        [InlineData("Ab1")]
        public async Task ExecutarAsync_QuandoSenhaInvalida_DeveLancarExcecao(string senha)
        {
            var (service, usuarioRepoMock, passwordServiceMock) = CriarService();

            var dto = new RegistrarUsuarioDto("Novo Aluno", "aluno@email.com", senha);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should().ThrowAsync<InvalidOperationException>();

            passwordServiceMock.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
            usuarioRepoMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Usuario>()), Times.Never);
        }


        [Fact]
        public async Task ExecutarAsync_SemCodigoDeProfessor_DeveCriarAluno()
        {
            var (service, usuarioRepoMock, _) = CriarService();

            var dto = new RegistrarUsuarioDto("Fulano", "fulano@email.com", "SenhaAbc123", null);

            await service.ExecutarAsync(dto);

            usuarioRepoMock.Verify(repo => repo.AdicionarAsync(It.Is<Usuario>(u =>
                u.Tipo == TipoUsuario.Aluno)), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_ComCodigoDeProfessorCorreto_DeveCriarProfessor()
        {
            var (service, usuarioRepoMock, _) = CriarService();

            var dto = new RegistrarUsuarioDto(
                "Profa. Teste", "profa@email.com", "SenhaAbc123", CodigoProfessorConfigurado);

            await service.ExecutarAsync(dto);

            usuarioRepoMock.Verify(repo => repo.AdicionarAsync(It.Is<Usuario>(u =>
                u.Tipo == TipoUsuario.Professor)), Times.Once);
        }

        [InlineData("codigo-errado")]
        [InlineData("Admin")]
        [InlineData("3")]
        [InlineData("2")]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ExecutarAsync_ComCodigoDeProfessorIncorreto_DeveRecusarSemCriarUsuario(string codigo)
        {
            var (service, usuarioRepoMock, passwordServiceMock) = CriarService();

            var dto = new RegistrarUsuarioDto("Intruso", "intruso@email.com", "SenhaAbc123", codigo);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Código de professor inválido.");

            usuarioRepoMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Usuario>()), Times.Never);
            passwordServiceMock.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task ExecutarAsync_QuandoCodigoNaoEstaConfigurado_NaoPermiteCriarProfessor(string? configurado)
        {
            var (service, usuarioRepoMock, _) = CriarService(codigoProfessor: configurado);

            var dto = new RegistrarUsuarioDto("Intruso", "intruso@email.com", "SenhaAbc123", "qualquer-coisa");

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Código de professor inválido.");

            usuarioRepoMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public void RegistrarUsuarioDto_NaoDeveExporPropriedadeDePapel()
        {
            var nomes = typeof(RegistrarUsuarioDto).GetProperties().Select(p => p.Name).ToList();

            nomes.Should().NotContain(n =>
                n.Contains("Tipo", StringComparison.OrdinalIgnoreCase) ||
                n.Contains("Role", StringComparison.OrdinalIgnoreCase) ||
                n.Contains("Papel", StringComparison.OrdinalIgnoreCase) ||
                n.Contains("Admin", StringComparison.OrdinalIgnoreCase));
        }
    }
}