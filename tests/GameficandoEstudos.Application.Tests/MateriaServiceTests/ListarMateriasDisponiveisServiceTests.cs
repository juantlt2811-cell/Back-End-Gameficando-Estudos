using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services;
using GameficandoEstudos.Application.Services.MateriaService;
using GameficandoEstudos.Domain.Entities;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Tests.Services
{
    public class ListarMateriasDisponiveisServiceTests
    {
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly ListarMateriasDisponiveisService _service;

        public ListarMateriasDisponiveisServiceTests()
        {
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _service = new ListarMateriasDisponiveisService(_materiaRepositoryMock.Object);
        }

        [Fact]
        public async Task ExecutarAsync_DeveRetornarTodasAsMateriasDoSistema_MapeadasParaDto()
        {
            var materiasDoBanco = new List<Materia>
            {
                new Materia("Matemática", 40, "Fácil", 1),
                new Materia("História", 40, "Fácil", 2),
                new Materia("Geografia", 40, "Média", 3)
            };

            _materiaRepositoryMock.Setup(repo => repo.ObterTodasAsync())
                                  .ReturnsAsync(materiasDoBanco);

            var resultado = await _service.ExecutarAsync();

            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(3);

            resultado.First().IdProfessor.Should().Be(1);
            resultado.Last().Nome.Should().Be("Geografia");

            _materiaRepositoryMock.Verify(repo => repo.ObterTodasAsync(), Times.Once);
        }
    }
}