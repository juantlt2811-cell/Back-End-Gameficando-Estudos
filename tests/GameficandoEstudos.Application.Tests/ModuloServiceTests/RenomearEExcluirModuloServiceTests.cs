using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.ModuloService;
using GameficandoEstudos.Domain.Entities;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.ModuloServiceTests
{
    public class RenomearEExcluirModuloServiceTests
    {
        private const int IdMateria = 10;
        private const int IdModulo = 7;
        private const int IdDono = 1;

        private readonly Mock<IModuloRepository> _moduloRepositoryMock;
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly Mock<IAuditoriaService> _auditoriaServiceMock;
        private readonly Modulo _modulo;
        private readonly RenomearModuloService _renomearService;
        private readonly ExcluirModuloService _excluirService;

        public RenomearEExcluirModuloServiceTests()
        {
            _moduloRepositoryMock = new Mock<IModuloRepository>();
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _auditoriaServiceMock = new Mock<IAuditoriaService>();

            _modulo = new Modulo("Condicionais", IdMateria);
            typeof(Modulo).GetProperty("Id")?.SetValue(_modulo, IdModulo);

            _moduloRepositoryMock.Setup(r => r.ObterPorIdAsync(IdModulo)).ReturnsAsync(_modulo);
            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(IdMateria))
                .ReturnsAsync(new Materia("Lógica", 40, "Fácil", IdDono));

            _renomearService = new RenomearModuloService(
                _moduloRepositoryMock.Object, _materiaRepositoryMock.Object, _auditoriaServiceMock.Object);
            _excluirService = new ExcluirModuloService(
                _moduloRepositoryMock.Object, _materiaRepositoryMock.Object, _auditoriaServiceMock.Object);
        }

        [Fact]
        public async Task Renomear_QuandoModuloNaoExiste_DeveLancarInvalidOperation()
        {
            Func<Task> act = async () => await _renomearService.ExecutarAsync(999, new RenomearModuloDto("Laços"), IdDono);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Módulo não encontrado.");
        }

        [Fact]
        public async Task Renomear_QuandoProfessorNaoEhDono_DeveLancarUnauthorizedSemAlterar()
        {
            Func<Task> act = async () => await _renomearService.ExecutarAsync(IdModulo, new RenomearModuloDto("Laços"), idProfessorLogado: 2);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _modulo.Nome.Should().Be("Condicionais");
            _moduloRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Modulo>()), Times.Never);
        }

        [Fact]
        public async Task Renomear_QuandoOutroModuloJaTemONome_DeveLancarSemSalvar()
        {
            _moduloRepositoryMock.Setup(r => r.ExisteComNomeAsync(IdMateria, "Laços", IdModulo)).ReturnsAsync(true);

            Func<Task> act = async () => await _renomearService.ExecutarAsync(IdModulo, new RenomearModuloDto("Laços"), IdDono);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Já existe um módulo com esse nome nesta matéria.");
            _moduloRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Modulo>()), Times.Never);
        }

        [Fact]
        public async Task Renomear_QuandoValido_DeveSalvarNovoNomeERegistrarAuditoria()
        {
            await _renomearService.ExecutarAsync(IdModulo, new RenomearModuloDto(" Laços "), IdDono);

            _modulo.Nome.Should().Be("Laços");
            _moduloRepositoryMock.Verify(r => r.AtualizarAsync(_modulo), Times.Once);
            _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                IdDono, "Modulo.Renomear", "Modulo", IdModulo.ToString(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task Excluir_QuandoProfessorNaoEhDono_DeveLancarUnauthorizedSemRemover()
        {
            Func<Task> act = async () => await _excluirService.ExecutarAsync(IdModulo, idProfessorLogado: 2);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _moduloRepositoryMock.Verify(r => r.RemoverAsync(It.IsAny<Modulo>()), Times.Never);
        }

        [Fact]
        public async Task Excluir_QuandoValido_DeveRemoverERegistrarAuditoria()
        {
            await _excluirService.ExecutarAsync(IdModulo, IdDono);

            _moduloRepositoryMock.Verify(r => r.RemoverAsync(_modulo), Times.Once);
            _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                IdDono, "Modulo.Excluir", "Modulo", IdModulo.ToString(), It.IsAny<string>()), Times.Once);
        }
    }
}
