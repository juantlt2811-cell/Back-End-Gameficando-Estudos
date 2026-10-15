using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.AtividadeService;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.AtividadeServiceTests
{
    public class GeradorAtividadeServiceTests
    {
        [Fact]
        public async Task GerarAtividadeAsync_ComFontes_DeveRetornarAtividadeDaIAESalvar()
        {
            int idMateriaFake = 1;
            string temaFake = "memória RAM";
            int idProfessorLogado = 1;
            var tipoAtividade = TipoAtividade.MultiplaEscolha;

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            var mockVectorSearch = new Mock<IVectorSearchService>();
            var mockLlmService = new Mock<ILlmService>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Materia("Redes", 1, "Baixa", 1));

            mockVectorSearch
                .Setup(v => v.BuscarChunksRelevantesAsync(idMateriaFake, temaFake))
                .ReturnsAsync(new List<string> { "A memória RAM é volátil." });

            mockLlmService
                .Setup(ia => ia.GerarRespostaAsync(It.IsAny<string>()))
                .ReturnsAsync("{ \"conteudo\": \"O que é RAM?\", \"gabarito\": \"Memória volátil\" }");

            atividadeRepositoryMock
                .Setup(repo => repo.AdicionarAsync(It.IsAny<Atividade>()))
                .ReturnsAsync((Atividade a) => a);

            var service = new GeradorAtividadeService(
                mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object,
                materiaRepositoryMock.Object, auditoriaServiceMock.Object);

            var resultado = await service.GerarAtividadeAsync(idMateriaFake, temaFake, tipoAtividade, idProfessorLogado);

            resultado.Should().NotBeNullOrEmpty();
            resultado.Should().Contain("\"conteudo\"");

            atividadeRepositoryMock.Verify(repo => repo.AdicionarAsync(It.Is<Atividade>(a =>
                a.Conteudo == "O que é RAM?" &&
                a.Gabarito == "Memória volátil" &&
                a.Tipo == TipoAtividade.MultiplaEscolha
            )), Times.Once);

            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                idProfessorLogado, "Atividade.Gerar", "Atividade", It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GerarAtividadeAsync_SemFontes_DeveLancarExcecao()
        {
            int idMateriaFake = 1;
            string temaFake = "tema inexistente";
            int idProfessorLogado = 1;
            var tipoAtividade = TipoAtividade.MultiplaEscolha;

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            var mockVectorSearch = new Mock<IVectorSearchService>();
            var mockLlmService = new Mock<ILlmService>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Materia("Redes", 1, "Baixa", 1));

            mockVectorSearch
                .Setup(v => v.BuscarChunksRelevantesAsync(idMateriaFake, temaFake))
                .ReturnsAsync(new List<string>());

            var service = new GeradorAtividadeService(
                mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object,
                materiaRepositoryMock.Object, auditoriaServiceMock.Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(idMateriaFake, temaFake, tipoAtividade, idProfessorLogado);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Nenhuma fonte encontrada para essa matéria e tema");

            mockLlmService.Verify(ia => ia.GerarRespostaAsync(It.IsAny<string>()), Times.Never);
            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GerarAtividadeAsync_MateriaNaoEncontrada_DeveLancarExcecao()
        {
            int idMateriaFake = 999;
            string temaFake = "memória RAM";
            int idProfessorLogado = 1;
            var tipoAtividade = TipoAtividade.MultiplaEscolha;

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            var mockVectorSearch = new Mock<IVectorSearchService>();
            var mockLlmService = new Mock<ILlmService>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Materia?)null);

            var service = new GeradorAtividadeService(
                mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object,
                materiaRepositoryMock.Object, auditoriaServiceMock.Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(idMateriaFake, temaFake, tipoAtividade, idProfessorLogado);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Matéria não encontrada");

            mockVectorSearch.Verify(v => v.BuscarChunksRelevantesAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            mockLlmService.Verify(ia => ia.GerarRespostaAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GerarAtividadeAsync_LLMRetornarJsonInvalido_DeveLancarExcecao()
        {
            int idMateriaFake = 1;
            string temaFake = "memória RAM";
            int idProfessorLogado = 1;
            var tipoAtividade = TipoAtividade.Dissertativa;

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            var mockVectorSearch = new Mock<IVectorSearchService>();
            var mockLlmService = new Mock<ILlmService>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Materia("Redes", 1, "Baixa", 1));

            mockVectorSearch
                .Setup(v => v.BuscarChunksRelevantesAsync(idMateriaFake, temaFake))
                .ReturnsAsync(new List<string> { "A memória RAM é volátil." });

            mockLlmService
                .Setup(ia => ia.GerarRespostaAsync(It.IsAny<string>()))
                .ReturnsAsync("Claro, aqui está a sua atividade: O que é RAM?");

            var service = new GeradorAtividadeService(
                mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object,
                materiaRepositoryMock.Object, auditoriaServiceMock.Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(idMateriaFake, temaFake, tipoAtividade, idProfessorLogado);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*LLM não retornou um JSON válido*");
        }
    }
}