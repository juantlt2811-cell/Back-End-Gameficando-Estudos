using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.RankingService;
using GameficandoEstudos.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.RankingServiceTests
{
    public class ObterEstatisticasMateriaServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoMateriaNaoExiste_DeveLancarInvalidOperation()
        {
            var rankingRepoMock = new Mock<IRankingRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();

            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync((Materia?)null);

            var service = new ObterEstatisticasMateriaService(rankingRepoMock.Object, materiaRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(idMateria: 10, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Matéria não encontrada.");

            rankingRepoMock.Verify(r => r.ObterEstatisticasPorMateriaAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorNaoEhDonoDaMateria_DeveLancarUnauthorized()
        {
            var rankingRepoMock = new Mock<IRankingRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();

            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 2);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(materia);

            var service = new ObterEstatisticasMateriaService(rankingRepoMock.Object, materiaRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(idMateria: 10, idProfessorLogado: 999);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Você não tem permissão para ver estatísticas de matérias de outros professores.");

            rankingRepoMock.Verify(r => r.ObterEstatisticasPorMateriaAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorEhDonoDaMateria_DeveRetornarEstatisticas()
        {
            var rankingRepoMock = new Mock<IRankingRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();

            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 2);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(materia);

            var estatisticasEsperadas = new List<EstatisticaAlunoMateriaDto>
            {
                new("Aluno A", SessoesConcluidas: 3, TotalTentativas: 15, PercentualAcerto: 73.3),
            };

            rankingRepoMock
                .Setup(r => r.ObterEstatisticasPorMateriaAsync(10))
                .ReturnsAsync(estatisticasEsperadas);

            var service = new ObterEstatisticasMateriaService(rankingRepoMock.Object, materiaRepoMock.Object);

            var resultado = await service.ExecutarAsync(idMateria: 10, idProfessorLogado: 2);

            resultado.Should().BeEquivalentTo(estatisticasEsperadas);
            rankingRepoMock.Verify(r => r.ObterEstatisticasPorMateriaAsync(10), Times.Once);
        }
    }
}