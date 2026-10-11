using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services;
using GameficandoEstudos.Application.Services.MateriaService;
using GameficandoEstudos.Domain.Entities;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Tests.Services.MateriaService
{
    public class BuscarMateriaPorIdServiceTests
    {
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly BuscarMateriaPorIdService _service;

        public BuscarMateriaPorIdServiceTests()
        {
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _service = new BuscarMateriaPorIdService(_materiaRepositoryMock.Object);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMateriaExisteEProfessorEhDono_DeveRetornarMateriaDto()
        {
            var idMateria = 1;
            var idProfessor = 10;

            var materia = new Materia("Engenharia de Software", 80, "Média", idProfessor);

            _materiaRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idMateria))
                                  .ReturnsAsync(materia);

            var resultado = await _service.ExecutarAsync(idMateria, idProfessor);

            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Engenharia de Software");
            resultado.CargaHoraria.Should().Be(80);
            resultado.IdProfessor.Should().Be(idProfessor);

            _materiaRepositoryMock.Verify(repo => repo.ObterPorIdAsync(idMateria), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMateriaNaoExiste_DeveLancarUnauthorizedAccessException()
        {
            var idMateria = 999;
            var idProfessor = 10;

            _materiaRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idMateria))
                                  .ReturnsAsync((Materia)null);

            Func<Task> act = async () => await _service.ExecutarAsync(idMateria, idProfessor);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("Matéria não encontrada ou acesso negado.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMateriaPertenceAOutroProfessor_DeveLancarUnauthorizedAccessException()
        {
            var idMateria = 1;
            var idProfessorLogado = 10; 
            var idProfessorDono = 20;   

            var materiaDeOutro = new Materia("Redes de Computadores", 60, "Difícil", idProfessorDono);

            _materiaRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idMateria))
                                  .ReturnsAsync(materiaDeOutro);

            Func<Task> act = async () => await _service.ExecutarAsync(idMateria, idProfessorLogado);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("Matéria não encontrada ou acesso negado.");
        }
    }
}