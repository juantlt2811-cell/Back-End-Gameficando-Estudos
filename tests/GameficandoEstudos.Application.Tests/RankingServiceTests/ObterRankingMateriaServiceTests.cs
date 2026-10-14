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
    public class ObterRankingMateriaServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoMateriaNaoExiste_DeveLancarInvalidOperation()
        {
            var rankingRepoMock = new Mock<IRankingRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();

            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync((Materia?)null);

            var service = new ObterRankingMateriaService(rankingRepoMock.Object, materiaRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(10);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Matéria não encontrada.");

            rankingRepoMock.Verify(
                r => r.ObterRankingPorMateriaAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMateriaExiste_DeveChamarRepositorioComLimiteEMinimoCorretos()
        {
            var rankingRepoMock = new Mock<IRankingRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();

            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 2);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(materia);

            var rankingEsperado = new List<RankingMateriaDto>
            {
                new(1, "Aluno A", 5, 4, 80.0),
            };

            rankingRepoMock
                .Setup(r => r.ObterRankingPorMateriaAsync(10, 20, 3))
                .ReturnsAsync(rankingEsperado);

            var service = new ObterRankingMateriaService(rankingRepoMock.Object, materiaRepoMock.Object);

            var resultado = await service.ExecutarAsync(10);

            resultado.Should().BeEquivalentTo(rankingEsperado);
            rankingRepoMock.Verify(r => r.ObterRankingPorMateriaAsync(10, 20, 3), Times.Once);
        }
    }
}