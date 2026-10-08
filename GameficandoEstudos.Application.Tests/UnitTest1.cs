using Xunit;
using Moq;
using FluentAssertions;
using GameficandoEstudos.Application.Services;
using GameficandoEstudos.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameficandoEstudos.Application.Tests
{
    public class GeradorAtividadeServiceTests
    {
        [Fact]
        public async Task GerarAtividadeAsync_ComFontes_DeveRetornarAtividadeDaIA()
        {
            
            int idMateriaFake = 1;
            string temaFake = "memória RAM";

            
            var mockVectorSearch = new Mock<IVectorSearchService>();
            
            mockVectorSearch
                .Setup(v => v.BuscarChunksRelevantesAsync(idMateriaFake, temaFake))
                .ReturnsAsync(new List<string> { "A memória RAM é volátil." });

            
            var mockLlmService = new Mock<ILlmService>();
            
            mockLlmService
                .Setup(ia => ia.GerarRespostaAsync(It.IsAny<string>()))
                .ReturnsAsync("{ \"pergunta\": \"O que é RAM?\" }");

            var service = new GeradorAtividadeService(mockVectorSearch.Object, mockLlmService.Object);

            var resultado = await service.GerarAtividadeAsync(idMateriaFake, temaFake);

            resultado.Should().NotBeNullOrEmpty();
            resultado.Should().Contain("{ \"pergunta\"");


        }
        [Fact]
        public async Task GerarAtividadeAsync_SemFontes_DeveLancarExcecao()
        {
            int idMateriaFake = 1;
            string temaFake = "tema inexistente";


            var mockVectorSearch = new Mock<IVectorSearchService>();


            mockVectorSearch
                .Setup(v => v.BuscarChunksRelevantesAsync(idMateriaFake, temaFake))
                .ReturnsAsync(new List<string>());


            var mockLlmService = new Mock<ILlmService>();

            var service = new GeradorAtividadeService(mockVectorSearch.Object, mockLlmService.Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(idMateriaFake, temaFake);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Nenhuma fonte encontrada para essa matéria e tema");

            mockLlmService.Verify(ia => ia.GerarRespostaAsync(It.IsAny<string>()), Times.Never);
        }
    }
}