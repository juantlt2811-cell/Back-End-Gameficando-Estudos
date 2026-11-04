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
                .Setup(v => v.BuscarChunksRelevantesAsync(idMateriaFake, temaFake, null))
                .ReturnsAsync(new List<string> { "A memória RAM é volátil." });

            mockLlmService
                .Setup(ia => ia.GerarRespostaAsync(It.IsAny<string>()))
                .ReturnsAsync("{ \"enunciado\": \"O que é RAM?\", \"alternativas\": [\"A) Memória volátil\", \"B) Memória permanente\"], \"gabarito\": \"A\" }");

            atividadeRepositoryMock
                .Setup(repo => repo.AdicionarAsync(It.IsAny<Atividade>()))
                .ReturnsAsync((Atividade a) => a);

            var service = new GeradorAtividadeService(
                mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object,
                materiaRepositoryMock.Object, auditoriaServiceMock.Object, new Mock<IModuloRepository>().Object);

            var resultado = await service.GerarAtividadeAsync(idMateriaFake, temaFake, tipoAtividade, idProfessorLogado);

            resultado.Conteudo.Should().Be("O que é RAM?");
            resultado.Tipo.Should().Be("MultiplaEscolha");
            resultado.IdMateria.Should().Be(idMateriaFake);
            resultado.Alternativas.Should().HaveCount(2);
            resultado.Alternativas.Should().Contain(a => a.EndsWith("Memória volátil"));
            resultado.Alternativas.Should().Contain(a => a.EndsWith("Memória permanente"));

            var letras = new[] { "A", "B" };
            resultado.Gabarito.Should().BeOneOf(letras);
            int indiceGabarito = Array.IndexOf(letras, resultado.Gabarito);
            resultado.Alternativas[indiceGabarito].Should().EndWith("Memória volátil");

            atividadeRepositoryMock.Verify(repo => repo.AdicionarAsync(It.Is<Atividade>(a =>
                a.Conteudo == "O que é RAM?" &&
                a.Tipo == TipoAtividade.MultiplaEscolha &&
                a.Alternativas.Count == 2
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
                .Setup(v => v.BuscarChunksRelevantesAsync(idMateriaFake, temaFake, null))
                .ReturnsAsync(new List<string>());

            var service = new GeradorAtividadeService(
                mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object,
                materiaRepositoryMock.Object, auditoriaServiceMock.Object, new Mock<IModuloRepository>().Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(idMateriaFake, temaFake, tipoAtividade, idProfessorLogado);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Nenhuma fonte encontrada para essa matéria e tema");

            mockLlmService.Verify(ia => ia.GerarRespostaAsync(It.IsAny<string>()), Times.Never);
            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GerarAtividadeAsync_TemaForaDoConteudo_DeveLancarExcecao()
        {
            int idMateriaFake = 1;
            string temaFake = "Velozes e Furiosos";
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
                .Setup(v => v.BuscarChunksRelevantesAsync(idMateriaFake, temaFake, null))
                .ReturnsAsync(new List<string> { "A memória RAM é volátil." });

            mockLlmService
                .Setup(ia => ia.GerarRespostaAsync(It.IsAny<string>()))
                .ReturnsAsync("{ \"enunciado\": \"\", \"alternativas\": [], \"gabarito\": \"\", \"temaForaDoConteudo\": true }");

            var service = new GeradorAtividadeService(
                mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object,
                materiaRepositoryMock.Object, auditoriaServiceMock.Object, new Mock<IModuloRepository>().Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(idMateriaFake, temaFake, tipoAtividade, idProfessorLogado);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*não foi encontrado no material*");

            atividadeRepositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Atividade>()), Times.Never);
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
                materiaRepositoryMock.Object, auditoriaServiceMock.Object, new Mock<IModuloRepository>().Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(idMateriaFake, temaFake, tipoAtividade, idProfessorLogado);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Matéria não encontrada");

            mockVectorSearch.Verify(v => v.BuscarChunksRelevantesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
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
                .Setup(v => v.BuscarChunksRelevantesAsync(idMateriaFake, temaFake, null))
                .ReturnsAsync(new List<string> { "A memória RAM é volátil." });

            mockLlmService
                .Setup(ia => ia.GerarRespostaAsync(It.IsAny<string>()))
                .ReturnsAsync("Claro, aqui está a sua atividade: O que é RAM?");

            var service = new GeradorAtividadeService(
                mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object,
                materiaRepositoryMock.Object, auditoriaServiceMock.Object, new Mock<IModuloRepository>().Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(idMateriaFake, temaFake, tipoAtividade, idProfessorLogado);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*LLM não retornou um JSON válido*");
        }

        [Fact]
        public async Task GerarAtividadeAsync_ProfessorQueNaoEhDonoDaMateria_DeveLancarUnauthorizedSemBuscarFontes()
        {
            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var mockVectorSearch = new Mock<IVectorSearchService>();
            var mockLlmService = new Mock<ILlmService>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(1))
                .ReturnsAsync(new Materia("Redes", 1, "Baixa", idProfessor: 2));

            var service = new GeradorAtividadeService(
                mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object,
                materiaRepositoryMock.Object, new Mock<IAuditoriaService>().Object, new Mock<IModuloRepository>().Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(1, "memória RAM", TipoAtividade.Dissertativa, idProfessorLogado: 1);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();

            mockVectorSearch.Verify(v => v.BuscarChunksRelevantesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
            mockLlmService.Verify(ia => ia.GerarRespostaAsync(It.IsAny<string>()), Times.Never);
            atividadeRepositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Atividade>()), Times.Never);
        }

        [Fact]
        public async Task GerarAtividadeAsync_ModuloDeOutraMateria_DeveLancarExcecaoSemBuscarFontes()
        {
            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var moduloRepositoryMock = new Mock<IModuloRepository>();
            var mockVectorSearch = new Mock<IVectorSearchService>();
            var mockLlmService = new Mock<ILlmService>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(1))
                .ReturnsAsync(new Materia("Redes", 1, "Baixa", 1));
            moduloRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(7))
                .ReturnsAsync(new Modulo("Módulo de outra matéria", idMateria: 99));

            var service = new GeradorAtividadeService(
                mockVectorSearch.Object, mockLlmService.Object, new Mock<IAtividadeRepository>().Object,
                materiaRepositoryMock.Object, new Mock<IAuditoriaService>().Object, moduloRepositoryMock.Object);

            Func<Task> act = async () => await service.GerarAtividadeAsync(1, "memória RAM", TipoAtividade.Dissertativa, 1, idModulo: 7);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Módulo não encontrado nesta matéria.");

            mockVectorSearch.Verify(v => v.BuscarChunksRelevantesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int?>()), Times.Never);
            mockLlmService.Verify(ia => ia.GerarRespostaAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GerarAtividadeAsync_ComModulo_DeveBuscarSoNoModuloESalvarAtividadeNele()
        {
            int idMateria = 1;
            int idModulo = 7;
            string tema = "estruturas condicionais";

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var moduloRepositoryMock = new Mock<IModuloRepository>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            var mockVectorSearch = new Mock<IVectorSearchService>();
            var mockLlmService = new Mock<ILlmService>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(idMateria))
                .ReturnsAsync(new Materia("Lógica", 1, "Baixa", 1));
            moduloRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(idModulo))
                .ReturnsAsync(new Modulo("Condicionais", idMateria));
            mockVectorSearch
                .Setup(v => v.BuscarChunksRelevantesAsync(idMateria, tema, idModulo))
                .ReturnsAsync(new List<string> { "O if executa um bloco quando a condição é verdadeira." });
            mockLlmService
                .Setup(ia => ia.GerarRespostaAsync(It.IsAny<string>()))
                .ReturnsAsync("{ \"enunciado\": \"Explique o if.\", \"alternativas\": [], \"gabarito\": \"Executa um bloco se a condição for verdadeira.\", \"temaForaDoConteudo\": false }");
            atividadeRepositoryMock
                .Setup(repo => repo.AdicionarAsync(It.IsAny<Atividade>()))
                .ReturnsAsync((Atividade a) => a);

            var service = new GeradorAtividadeService(
                mockVectorSearch.Object, mockLlmService.Object, atividadeRepositoryMock.Object,
                materiaRepositoryMock.Object, auditoriaServiceMock.Object, moduloRepositoryMock.Object);

            var resultado = await service.GerarAtividadeAsync(idMateria, tema, TipoAtividade.Dissertativa, 1, idModulo);

            resultado.IdModulo.Should().Be(idModulo);
            mockVectorSearch.Verify(v => v.BuscarChunksRelevantesAsync(idMateria, tema, idModulo), Times.Once);
            atividadeRepositoryMock.Verify(repo => repo.AdicionarAsync(It.Is<Atividade>(a => a.IdModulo == idModulo)), Times.Once);
        }
    }
}