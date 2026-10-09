using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services;
using GameficandoEstudos.Domain.Entities;
using Moq;
using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Tests
{
    public class GeradorAtividadeServiceTests
    {
        [Fact]
        public async Task GerarAtividadeAsync_ComFontes_DeveRetornarAtividadeDaIA()
        {
            
            int idMateriaFake = 1;
            string temaFake = "memória RAM";

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Materia("Redes", 1, "Baixa", 1)); 

            var mockVectorSearch = new Mock<IVectorSearchService>();
            
            mockVectorSearch
                .Setup(v => v.BuscarChunksRelevantesAsync(idMateriaFake, temaFake))
                .ReturnsAsync(new List<string> { "A memória RAM é volátil." });

            

            var mockLlmService = new Mock<ILlmService>();
            
            mockLlmService
                .Setup(ia => ia.GerarRespostaAsync(It.IsAny<string>()))
                .ReturnsAsync("{ \"pergunta\": \"O que é RAM?\" }");

            var service = new GeradorAtividadeService(mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object, materiaRepositoryMock.Object);

            var resultado = await service.GerarAtividadeAsync(idMateriaFake, temaFake, "Quiz");

            resultado.Should().NotBeNullOrEmpty();
            resultado.Should().Contain("{ \"pergunta\"");


        }
        [Fact]
        public async Task GerarAtividadeAsync_SemFontes_DeveLancarExcecao()
        {
            int idMateriaFake = 1;
            string temaFake = "tema inexistente";

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Materia("Redes", 1, "Baixa", 1));


            var mockVectorSearch = new Mock<IVectorSearchService>();


            mockVectorSearch
                .Setup(v => v.BuscarChunksRelevantesAsync(idMateriaFake, temaFake))
                .ReturnsAsync(new List<string>());


            var mockLlmService = new Mock<ILlmService>();

            var service = new GeradorAtividadeService(mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object, materiaRepositoryMock.Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(idMateriaFake, temaFake, "Quiz");

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Nenhuma fonte encontrada para essa matéria e tema");

            mockLlmService.Verify(ia => ia.GerarRespostaAsync(It.IsAny<string>()), Times.Never);
        }
        [Fact]
        public async Task GerarAtividadeAsync_MateriaNaoEncontrada_DeveLancarExcecao()
        {
            int idMateriaFake = 999;
            string temaFake = "memória RAM";

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Materia?)null);

            var mockVectorSearch = new Mock<IVectorSearchService>();

            var mockLlmService = new Mock<ILlmService>();

            var service = new GeradorAtividadeService(mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object, materiaRepositoryMock.Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(idMateriaFake, temaFake, "Quiz");
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Matéria não encontrada");

            mockVectorSearch.Verify(v => v.BuscarChunksRelevantesAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockLlmService.Verify(ia => ia.GerarRespostaAsync(It.IsAny<string>()), Times.Never);
        }
        [Fact]
        public async Task GerarAtividadeAsync_TipoNaoDigitado_DeveLancarExcecao()
        {
            int idMateriaFake = 1;
            string temaFake = "memória RAM";

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Materia("Redes", 1, "Baixa", 1));

            var mockVectorSearch = new Mock<IVectorSearchService>();
            var mockLlmService = new Mock<ILlmService>();

            var service = new GeradorAtividadeService(mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object, materiaRepositoryMock.Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(idMateriaFake, temaFake, null);

            await act.Should().ThrowAsync<ArgumentNullException>()
                .WithMessage("Tipo de atividade não informado (Parameter 'tipoAtividade')");

            mockVectorSearch.Verify(v => v.BuscarChunksRelevantesAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockLlmService.Verify(ia => ia.GerarRespostaAsync(It.IsAny<string>()), Times.Never);
        }
    }
}