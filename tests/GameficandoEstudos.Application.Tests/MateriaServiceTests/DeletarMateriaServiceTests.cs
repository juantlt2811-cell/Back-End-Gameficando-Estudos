using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.MateriaService;
using GameficandoEstudos.Domain.Entities;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Tests.Services
{
    public class DeletarMateriaServiceTests
    {
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly DeletarMateriaService _service;

        public DeletarMateriaServiceTests()
        {
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _service = new DeletarMateriaService(_materiaRepositoryMock.Object);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorEhDono_DeveDeletarMateria()
        {
            var idProfessor = 1;
            var idMateria = 10;
            var materiaExistente = new Materia("Química", 40, "Média", idProfessor);

            _materiaRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idMateria))
                                  .ReturnsAsync(materiaExistente);

            await _service.ExecutarAsync(idMateria, idProfessor);

            _materiaRepositoryMock.Verify(repo => repo.DeletarAsync(idMateria), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorNaoEhDono_DeveLancarExcecao()
        {
            
            var idMateria = 10;
            var idProfessorLogado = 2;
            var materiaDeOutroProfessor = new Materia("Química", 40, "Média", 1); // Pertence ao prof 1

            _materiaRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idMateria))
                                  .ReturnsAsync(materiaDeOutroProfessor);

            Func<Task> act = async () => await _service.ExecutarAsync(idMateria, idProfessorLogado);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _materiaRepositoryMock.Verify(repo => repo.DeletarAsync(It.IsAny<int>()), Times.Never);
        }
    }
}