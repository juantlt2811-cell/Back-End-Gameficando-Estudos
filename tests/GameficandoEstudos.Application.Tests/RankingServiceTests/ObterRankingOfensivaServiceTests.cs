using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.RankingService;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.RankingServiceTests
{
    public class ObterRankingOfensivaServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_DeveRetornarRankingDoRepositorioComLimitePadrao()
        {
            var rankingRepoMock = new Mock<IRankingRepository>();

            var rankingEsperado = new List<RankingOfensivaDto>
            {
                new(1, "Aluno A", 10, 15),
                new(2, "Aluno B", 8, 8),
            };

            rankingRepoMock.Setup(r => r.ObterRankingOfensivaAsync(20)).ReturnsAsync(rankingEsperado);

            var service = new ObterRankingOfensivaService(rankingRepoMock.Object);

            var resultado = await service.ExecutarAsync();

            resultado.Should().BeEquivalentTo(rankingEsperado);
            rankingRepoMock.Verify(r => r.ObterRankingOfensivaAsync(20), Times.Once);
        }
    }
}