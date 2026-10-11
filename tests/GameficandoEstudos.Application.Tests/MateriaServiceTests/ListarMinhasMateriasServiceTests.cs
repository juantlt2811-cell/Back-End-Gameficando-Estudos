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
    public class ListarMinhasMateriasServiceTests
    {
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly ListarMinhasMateriasService _service;

        public ListarMinhasMateriasServiceTests()
        {
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _service = new ListarMinhasMateriasService(_materiaRepositoryMock.Object);
        }

        [Fact]
        public async Task ExecutarAsync_DeveRetornarApenasMateriasDoProfessor_MapeadasParaDto()
        {
            var idProfessor = 1;

            var materiasDoBanco = new List<Materia>
            {
                new Materia("Algoritmos", 80, "Média", idProfessor),
                new Materia("Estrutura de Dados", 120, "Difícil", idProfessor)
            };

            _materiaRepositoryMock.Setup(repo => repo.ObterPorProfessorAsync(idProfessor))
                                  .ReturnsAsync(materiasDoBanco);

            var resultado = await _service.ExecutarAsync(idProfessor);

            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2);

            resultado.First().Nome.Should().Be("Algoritmos");
            resultado.Last().CargaHoraria.Should().Be(120);

            _materiaRepositoryMock.Verify(repo => repo.ObterPorProfessorAsync(idProfessor), Times.Once);
        }
    }
}