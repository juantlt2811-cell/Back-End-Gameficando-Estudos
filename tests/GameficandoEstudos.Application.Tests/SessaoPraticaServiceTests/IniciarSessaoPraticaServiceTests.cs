using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.SessaoPraticaService;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.SessaoPraticaServiceTests
{
    public class IniciarSessaoPraticaServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioNaoEhAluno_DeveLancarUnauthorized()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();

            var professor = new Usuario("Professor Teste", "professor@teste.com", "senha123", TipoUsuario.Professor);
            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(professor);

            var service = new IniciarSessaoPraticaService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object, sessaoRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idMateria: 10);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Apenas alunos podem iniciar sessões de prática.");

            materiaRepoMock.Verify(r => r.ObterPorIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoLimiteDiarioAtingido_DeveLancarSemConsultarMateria()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            typeof(PerfilGamificacao).GetProperty("AtividadesFeitasHoje")?.SetValue(aluno.Gamificacao, 3);
            typeof(PerfilGamificacao).GetProperty("UltimaAtividadeRealizadaEm")?.SetValue(aluno.Gamificacao, DateTime.UtcNow);

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);

            var service = new IniciarSessaoPraticaService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object, sessaoRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idMateria: 10);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Limite diário de matérias praticadas atingido. Volte amanhã!");

            materiaRepoMock.Verify(r => r.ObterPorIdAsync(It.IsAny<int>()), Times.Never);
            sessaoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<SessaoPratica>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMateriaNaoExiste_DeveLancarInvalidOperation()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync((Materia?)null);

            var service = new IniciarSessaoPraticaService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object, sessaoRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idMateria: 10);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Matéria não encontrada.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMateriaSemQuestoesAtivas_DeveLancarInvalidOperation()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 2);

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(materia);
            atividadeRepoMock.Setup(r => r.ObterAleatoriasPorMateriaAsync(10, 5))
                             .ReturnsAsync(new List<Atividade>());

            var service = new IniciarSessaoPraticaService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object, sessaoRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idMateria: 10);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Esta matéria ainda não possui questões disponíveis.");

            sessaoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<SessaoPratica>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoTudoOk_DeveCriarSessaoComQuestoesSemGabarito()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 2);

            var questoes = new List<Atividade>
            {
                new("Questao 1", TipoAtividade.MultiplaEscolha, "Conteudo 1", "A", idMateria: 10),
                new("Questao 2", TipoAtividade.Dissertativa, "Conteudo 2", "Gabarito 2", idMateria: 10),
                new("Questao 3", TipoAtividade.Codigo, "Conteudo 3", "Gabarito 3", idMateria: 10),
            };

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(materia);
            atividadeRepoMock.Setup(r => r.ObterAleatoriasPorMateriaAsync(10, 5)).ReturnsAsync(questoes);

            SessaoPratica? sessaoCriada = null;
            sessaoRepoMock
                .Setup(r => r.AdicionarAsync(It.IsAny<SessaoPratica>()))
                .Callback<SessaoPratica>(s => sessaoCriada = s)
                .ReturnsAsync((SessaoPratica s) => s);

            var service = new IniciarSessaoPraticaService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object, sessaoRepoMock.Object);

            var resultado = await service.ExecutarAsync(1, idMateria: 10);

            resultado.Questoes.Should().HaveCount(3);
            resultado.Questoes.Should().OnlyContain(q => q.Nome != null); 

            sessaoCriada.Should().NotBeNull();
            sessaoCriada!.IdUsuario.Should().Be(1);
            sessaoCriada.IdMateria.Should().Be(10);
            sessaoCriada.TotalQuestoes.Should().Be(3);

            sessaoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<SessaoPratica>()), Times.Once);
        }
    }
}