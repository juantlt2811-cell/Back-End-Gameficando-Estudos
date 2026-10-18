using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.SessaoPraticaService;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.SessaoPraticaServiceTests
{
    public class ResponderSessaoPraticaServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoRespostaVazia_DeveLancarArgumentException()
        {
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var llmServiceMock = new Mock<ILlmService>();

            var service = new ResponderSessaoPraticaService(
                sessaoRepoMock.Object, atividadeRepoMock.Object, llmServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idSessao: 1, idAtividade: 1, resposta: "   ");

            await act.Should().ThrowAsync<ArgumentException>();

            sessaoRepoMock.Verify(r => r.ObterPorIdComTentativasAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoSessaoNaoEncontrada_DeveLancarUnauthorized()
        {
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var llmServiceMock = new Mock<ILlmService>();

            sessaoRepoMock.Setup(r => r.ObterPorIdComTentativasAsync(1)).ReturnsAsync((SessaoPratica?)null);

            var service = new ResponderSessaoPraticaService(
                sessaoRepoMock.Object, atividadeRepoMock.Object, llmServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idSessao: 1, idAtividade: 1, resposta: "A");

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Sessão de prática não encontrada.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoSessaoPertenceAOutroUsuario_DeveLancarUnauthorized()
        {
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var llmServiceMock = new Mock<ILlmService>();

            var sessao = new SessaoPratica(idUsuario: 99, idMateria: 10, totalQuestoes: 3);
            sessaoRepoMock.Setup(r => r.ObterPorIdComTentativasAsync(1)).ReturnsAsync(sessao);

            var service = new ResponderSessaoPraticaService(
                sessaoRepoMock.Object, atividadeRepoMock.Object, llmServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idSessao: 1, idAtividade: 1, resposta: "A");

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Sessão de prática não encontrada.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoAtividadeNaoPertenceAMateriaDaSessao_DeveLancarInvalidOperation()
        {
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var llmServiceMock = new Mock<ILlmService>();

            var sessao = new SessaoPratica(idUsuario: 1, idMateria: 10, totalQuestoes: 3);
            var atividadeDeOutraMateria = new Atividade("Questao X", TipoAtividade.MultiplaEscolha, "Conteudo", "A", idMateria: 999);

            sessaoRepoMock.Setup(r => r.ObterPorIdComTentativasAsync(1)).ReturnsAsync(sessao);
            atividadeRepoMock.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(atividadeDeOutraMateria);

            var service = new ResponderSessaoPraticaService(
                sessaoRepoMock.Object, atividadeRepoMock.Object, llmServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idSessao: 1, idAtividade: 5, resposta: "A");

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Essa questão não pertence a esta sessão de prática.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMultiplaEscolhaCorreta_DeveRegistrarTentativaERetornarAprovado()
        {
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var llmServiceMock = new Mock<ILlmService>();

            var sessao = new SessaoPratica(idUsuario: 1, idMateria: 10, totalQuestoes: 3);
            var atividade = new Atividade("Questao 1", TipoAtividade.MultiplaEscolha, "Conteudo", "A", idMateria: 10);

            sessaoRepoMock.Setup(r => r.ObterPorIdComTentativasAsync(1)).ReturnsAsync(sessao);
            atividadeRepoMock.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(atividade);

            var service = new ResponderSessaoPraticaService(
                sessaoRepoMock.Object, atividadeRepoMock.Object, llmServiceMock.Object);

            var resultado = await service.ExecutarAsync(1, idSessao: 1, idAtividade: 5, resposta: "a");

            resultado.Aprovado.Should().BeTrue();
            sessao.Tentativas.Should().HaveCount(1);
            sessao.TotalAcertos.Should().Be(1);

            llmServiceMock.Verify(
                l => l.AvaliarRespostaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TipoAtividade>()), Times.Never);
            sessaoRepoMock.Verify(r => r.AtualizarAsync(sessao), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoDissertativa_DeveAvaliarComLlm()
        {
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var llmServiceMock = new Mock<ILlmService>();

            var sessao = new SessaoPratica(idUsuario: 1, idMateria: 10, totalQuestoes: 3);
            var atividade = new Atividade("O que é um FOR?", TipoAtividade.Dissertativa, "Explique o FOR", "Estrutura de repetição", idMateria: 10);

            sessaoRepoMock.Setup(r => r.ObterPorIdComTentativasAsync(1)).ReturnsAsync(sessao);
            atividadeRepoMock.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(atividade);
            llmServiceMock
                .Setup(l => l.AvaliarRespostaAsync(atividade.Conteudo, "Uma estrutura de repeticao", atividade.Gabarito, atividade.Tipo))
                .ReturnsAsync(new ResultadoAvaliacaoDto(100, "Correto!"));

            var service = new ResponderSessaoPraticaService(
                sessaoRepoMock.Object, atividadeRepoMock.Object, llmServiceMock.Object);

            var resultado = await service.ExecutarAsync(1, idSessao: 1, idAtividade: 5, resposta: "Uma estrutura de repeticao");

            resultado.Aprovado.Should().BeTrue();
            sessao.Tentativas.Should().HaveCount(1);
            llmServiceMock.Verify(
                 l => l.AvaliarRespostaAsync(atividade.Conteudo, "Uma estrutura de repeticao", atividade.Gabarito, atividade.Tipo), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoQuestaoJaRespondidaNestaSessao_DevePropagarExcecaoDoDominio()
        {
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var llmServiceMock = new Mock<ILlmService>();

            var sessao = new SessaoPratica(idUsuario: 1, idMateria: 10, totalQuestoes: 3);
            var atividade = new Atividade("Questao 1", TipoAtividade.MultiplaEscolha, "Conteudo", "A", idMateria: 10);

            sessaoRepoMock.Setup(r => r.ObterPorIdComTentativasAsync(1)).ReturnsAsync(sessao);
            atividadeRepoMock.Setup(r => r.ObterPorIdAsync(5)).ReturnsAsync(atividade);

            var service = new ResponderSessaoPraticaService(
                sessaoRepoMock.Object, atividadeRepoMock.Object, llmServiceMock.Object);

            await service.ExecutarAsync(1, idSessao: 1, idAtividade: 5, resposta: "A");

            Func<Task> act = async () => await service.ExecutarAsync(1, idSessao: 1, idAtividade: 5, resposta: "A");

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Essa questão já foi respondida nesta sessão.");
        }
    }
}