using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.MateriaService;
using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Tests.Services
{
    public class CriarMateriaServiceTests
    {
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly Mock<IAuditoriaService> _auditoriaServiceMock;
        private readonly CriarMateriaService _service;

        public CriarMateriaServiceTests()
        {
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _auditoriaServiceMock = new Mock<IAuditoriaService>();
            _service = new CriarMateriaService(_materiaRepositoryMock.Object, _auditoriaServiceMock.Object);
        }

        [Fact]
        public async Task ExecutarAsync_DeveCriarMateria_E_RetornarId()
        {
            var dto = new CriarMateriaDto("Matemática", 40, "Fácil");
            var idProfessor = 1;

            var materiaCriada = new Materia(dto.Nome, dto.CargaHoraria, dto.Dificuldade, idProfessor);

            _materiaRepositoryMock.Setup(repo => repo.AdicionarAsync(It.IsAny<Materia>()))
                                  .ReturnsAsync(materiaCriada);

            var resultado = await _service.ExecutarAsync(dto, idProfessor);

            _materiaRepositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Materia>()), Times.Once);
            resultado.Should().Be(materiaCriada.Id);

            _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                idProfessor, "Materia.Criar", "Materia", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }
    }
}