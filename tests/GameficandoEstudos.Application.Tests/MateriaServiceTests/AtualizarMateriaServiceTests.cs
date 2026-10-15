using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.MateriaService;
using GameficandoEstudos.Domain.Entities;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Tests.Services
{
    public class AtualizarMateriaServiceTests
    {
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly Mock<IAuditoriaService> _auditoriaServiceMock;
        private readonly AtualizarMateriaService _service;

        public AtualizarMateriaServiceTests()
        {
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _auditoriaServiceMock = new Mock<IAuditoriaService>();
            _service = new AtualizarMateriaService(_materiaRepositoryMock.Object, _auditoriaServiceMock.Object);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorEhDono_DeveAtualizarMateria()
        {
            var idProfessor = 1;
            var dto = new AtualizarMateriaDto(10, "Física Avançada", 60, "Difícil");
            var materiaExistente = new Materia("Física", 40, "Média", idProfessor);

            _materiaRepositoryMock.Setup(repo => repo.ObterPorIdAsync(dto.Id))
                                  .ReturnsAsync(materiaExistente);

            await _service.ExecutarAsync(dto, idProfessor);

            materiaExistente.Nome.Should().Be("Física Avançada");
            materiaExistente.CargaHoraria.Should().Be(60);
            _materiaRepositoryMock.Verify(repo => repo.AtualizarAsync(materiaExistente), Times.Once);

            _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                idProfessor, "Materia.Atualizar", "Materia", dto.Id.ToString(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorNaoEhDono_DeveLancarUnauthorizedAccessException()
        {
            var idProfessorLogado = 2;
            var idProfessorDono = 1;
            var dto = new AtualizarMateriaDto(10, "Hackeando", 10, "Fácil");

            var materiaExistente = new Materia("História", 40, "Fácil", idProfessorDono);

            _materiaRepositoryMock.Setup(repo => repo.ObterPorIdAsync(dto.Id))
                                  .ReturnsAsync(materiaExistente);

            Func<Task> act = async () => await _service.ExecutarAsync(dto, idProfessorLogado);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("Matéria não encontrada ou você não tem permissão para editá-la.");

            _materiaRepositoryMock.Verify(repo => repo.AtualizarAsync(It.IsAny<Materia>()), Times.Never);
            _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}