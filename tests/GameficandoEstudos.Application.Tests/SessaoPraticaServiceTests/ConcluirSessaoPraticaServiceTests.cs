using FluentAssertions;
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
    public class ConcluirSessaoPraticaServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoSessaoNaoEncontrada_DeveLancarUnauthorized()
        {
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();
            var usuarioRepoMock = new Mock<IUsuarioRepository>();

            sessaoRepoMock.Setup(r => r.ObterPorIdComTentativasAsync(1)).ReturnsAsync((SessaoPratica?)null);

            var service = new ConcluirSessaoPraticaService(sessaoRepoMock.Object, usuarioRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idSessao: 1);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Sessão de prática não encontrada.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoSessaoPertenceAOutroUsuario_DeveLancarUnauthorized()
        {
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();
            var usuarioRepoMock = new Mock<IUsuarioRepository>();

            var sessao = new SessaoPratica(idUsuario: 99, idMateria: 10, totalQuestoes: 3);
            sessaoRepoMock.Setup(r => r.ObterPorIdComTentativasAsync(1)).ReturnsAsync(sessao);

            var service = new ConcluirSessaoPraticaService(sessaoRepoMock.Object, usuarioRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idSessao: 1);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Sessão de prática não encontrada.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoSessaoSemTentativas_DevePropagarExcecaoDoDominio()
        {
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();
            var usuarioRepoMock = new Mock<IUsuarioRepository>();

            var sessao = new SessaoPratica(idUsuario: 1, idMateria: 10, totalQuestoes: 3);
            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);

            sessaoRepoMock.Setup(r => r.ObterPorIdComTentativasAsync(1)).ReturnsAsync(sessao);
            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);

            var service = new ConcluirSessaoPraticaService(sessaoRepoMock.Object, usuarioRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idSessao: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Responda ao menos uma questão antes de concluir a sessão.");

            usuarioRepoMock.Verify(r => r.AtualizarAsync(It.IsAny<Usuario>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoSessaoValida_DeveConcluirERegistrarGamificacao()
        {
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();
            var usuarioRepoMock = new Mock<IUsuarioRepository>();

            var sessao = new SessaoPratica(idUsuario: 1, idMateria: 10, totalQuestoes: 2);
            sessao.RegistrarTentativa(new TentativaResposta(
                idSessaoPratica: 1, idAtividade: 1, idUsuario: 1, idMateria: 10,
                respostaAluno: "A", pontuacao: 100, feedback: "Correto!"));
            sessao.RegistrarTentativa(new TentativaResposta(
                idSessaoPratica: 1, idAtividade: 2, idUsuario: 1, idMateria: 10,
                respostaAluno: "B", pontuacao: 0, feedback: "Incorreto."));

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);

            sessaoRepoMock.Setup(r => r.ObterPorIdComTentativasAsync(1)).ReturnsAsync(sessao);
            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);

            var service = new ConcluirSessaoPraticaService(sessaoRepoMock.Object, usuarioRepoMock.Object);

            var resumo = await service.ExecutarAsync(1, idSessao: 1);

            resumo.TotalRespondidas.Should().Be(2);
            resumo.TotalAcertos.Should().Be(1);
            resumo.PercentualAcerto.Should().Be(50);
            resumo.NotaMedia.Should().Be(50);
            resumo.OfensivaAtual.Should().Be(1);

            aluno.Gamificacao!.AtividadesFeitasHoje.Should().Be(1);

            sessaoRepoMock.Verify(r => r.AtualizarAsync(sessao), Times.Once);
            usuarioRepoMock.Verify(r => r.AtualizarAsync(aluno), Times.Once);
        }
    }
}