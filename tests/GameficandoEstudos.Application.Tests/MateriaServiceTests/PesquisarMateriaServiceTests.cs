using System.Collections.Generic;
using System.Linq;
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
    public class PesquisarMateriaServiceTests
    {
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly PesquisarMateriaService _service;

        public PesquisarMateriaServiceTests()
        {
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _service = new PesquisarMateriaService(_materiaRepositoryMock.Object);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoFiltrosForemNulosOuVazios_DeveRetornarListaVazia_E_NaoChamarBanco()
        {
            var resultado = await _service.ExecutarAsync(null, null);

            resultado.Should().BeEmpty();

            _materiaRepositoryMock.Verify(repo => repo.PesquisarAsync(It.IsAny<string?>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoFiltrosForemValidos_DeveRetornarListaMapeada()
        {
            var termoPesquisa = "Física";
            int? idProfessor = 1;

            var materiasDoBanco = new List<Materia>
            {
                new Materia("Física Quântica", 60, "Difícil", 1)
            };

            _materiaRepositoryMock.Setup(repo => repo.PesquisarAsync(termoPesquisa, idProfessor))
                                  .ReturnsAsync(materiasDoBanco);

            var resultado = await _service.ExecutarAsync(termoPesquisa, idProfessor);

            resultado.Should().NotBeEmpty().And.HaveCount(1);
            resultado.First().Nome.Should().Be("Física Quântica");
            resultado.First().IdProfessor.Should().Be(1);

            _materiaRepositoryMock.Verify(repo => repo.PesquisarAsync(termoPesquisa, idProfessor), Times.Once);
        }
    }
}