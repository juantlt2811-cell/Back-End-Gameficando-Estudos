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
    public class CriarModuloServiceTests
    {
        private const int IdMateria = 10;
        private const int IdDono = 1;

        private readonly Mock<IModuloRepository> _moduloRepositoryMock;
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly Mock<IAuditoriaService> _auditoriaServiceMock;
        private readonly CriarModuloService _service;

        public CriarModuloServiceTests()
        {
            _moduloRepositoryMock = new Mock<IModuloRepository>();
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _auditoriaServiceMock = new Mock<IAuditoriaService>();

            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(IdMateria))
                .ReturnsAsync(new Materia("Lógica", 40, "Fácil", IdDono));

            _service = new CriarModuloService(
                _moduloRepositoryMock.Object, _materiaRepositoryMock.Object, _auditoriaServiceMock.Object);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorNaoEhDono_DeveLancarUnauthorizedSemCriar()
        {
            Func<Task> act = async () => await _service.ExecutarAsync(new CriarModuloDto(IdMateria, "Condicionais"), idProfessorLogado: 2);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _moduloRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Modulo>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoNomeJaExisteNaMateria_DeveLancarSemCriar()
        {
            _moduloRepositoryMock.Setup(r => r.ExisteComNomeAsync(IdMateria, "Condicionais", null)).ReturnsAsync(true);

            Func<Task> act = async () => await _service.ExecutarAsync(new CriarModuloDto(IdMateria, "  Condicionais  "), IdDono);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Já existe um módulo com esse nome nesta matéria.");
            _moduloRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<Modulo>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoValido_DeveCriarComNomeLimpoERegistrarAuditoria()
        {
            Modulo? salvo = null;
            _moduloRepositoryMock
                .Setup(r => r.AdicionarAsync(It.IsAny<Modulo>()))
                .Callback<Modulo>(m => salvo = m)
                .ReturnsAsync((Modulo m) => m);

            var resultado = await _service.ExecutarAsync(new CriarModuloDto(IdMateria, "  Condicionais  "), IdDono);

            salvo.Should().NotBeNull();
            salvo!.Nome.Should().Be("Condicionais");
            salvo.IdMateria.Should().Be(IdMateria);

            resultado.Nome.Should().Be("Condicionais");
            resultado.IdMateria.Should().Be(IdMateria);
            resultado.QuantidadeAtividades.Should().Be(0);
            resultado.QuantidadeMateriais.Should().Be(0);

            _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                IdDono, "Modulo.Criar", "Modulo", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}
