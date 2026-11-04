using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.SessaoPraticaService;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
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
            atividadeRepoMock.Setup(r => r.ObterAleatoriasPorMateriaAsync(10, null, 5))
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
                ComId(new("Questao 1", TipoAtividade.MultiplaEscolha, "Conteudo 1", "A", idMateria: 10), 101),
                ComId(new("Questao 2", TipoAtividade.Dissertativa, "Conteudo 2", "Gabarito 2", idMateria: 10), 102),
                ComId(new("Questao 3", TipoAtividade.Codigo, "Conteudo 3", "Gabarito 3", idMateria: 10), 103),
            };

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(materia);
            atividadeRepoMock.Setup(r => r.ObterAleatoriasPorMateriaAsync(10, null, 5)).ReturnsAsync(questoes);

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
            sessaoCriada.IdModulo.Should().BeNull();
            sessaoCriada.IdsAtividades.Should().BeEquivalentTo(new[] { 101, 102, 103 });

            sessaoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<SessaoPratica>()), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_ComModuloETamanhoTodas_DeveSortearTodasDoModuloEGuardarNaSessao()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 2);

            var questoes = new List<Atividade>
            {
                ComId(new("Questao 1", TipoAtividade.Dissertativa, "Conteudo 1", "Gabarito 1", idMateria: 10, idModulo: 7), 201),
                ComId(new("Questao 2", TipoAtividade.Dissertativa, "Conteudo 2", "Gabarito 2", idMateria: 10, idModulo: 7), 202),
            };

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(materia);
            atividadeRepoMock.Setup(r => r.ObterAleatoriasPorMateriaAsync(10, 7, null)).ReturnsAsync(questoes);

            SessaoPratica? sessaoCriada = null;
            sessaoRepoMock
                .Setup(r => r.AdicionarAsync(It.IsAny<SessaoPratica>()))
                .Callback<SessaoPratica>(s => sessaoCriada = s)
                .ReturnsAsync((SessaoPratica s) => s);

            var service = new IniciarSessaoPraticaService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object, sessaoRepoMock.Object);

            var resultado = await service.ExecutarAsync(1, idMateria: 10, idModulo: 7, tamanho: TamanhoSessao.Todas);

            resultado.Questoes.Select(q => q.Id).Should().BeEquivalentTo(new[] { 201, 202 });
            sessaoCriada!.IdModulo.Should().Be(7);
            sessaoCriada.TotalQuestoes.Should().Be(2);
            sessaoCriada.IdsAtividades.Should().BeEquivalentTo(new[] { 201, 202 });
            atividadeRepoMock.Verify(r => r.ObterAleatoriasPorMateriaAsync(10, 7, null), Times.Once);
        }

        [Theory]
        [InlineData(TamanhoSessao.Curta, 5)]
        [InlineData(TamanhoSessao.Media, 10)]
        [InlineData(TamanhoSessao.Longa, 20)]
        public async Task ExecutarAsync_DevePedirAoRepositorioAQuantidadeDoTamanhoEscolhido(TamanhoSessao tamanho, int quantidadeEsperada)
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno));
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10))
                .ReturnsAsync(new Materia("Matematica", 40, "Facil", idProfessor: 2));
            atividadeRepoMock.Setup(r => r.ObterAleatoriasPorMateriaAsync(10, null, quantidadeEsperada))
                .ReturnsAsync(new List<Atividade> { ComId(new("Questao 1", TipoAtividade.MultiplaEscolha, "Conteudo 1", "A", idMateria: 10), 301) });
            sessaoRepoMock.Setup(r => r.AdicionarAsync(It.IsAny<SessaoPratica>())).ReturnsAsync((SessaoPratica s) => s);

            var service = new IniciarSessaoPraticaService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object, sessaoRepoMock.Object);

            await service.ExecutarAsync(1, idMateria: 10, tamanho: tamanho);

            atividadeRepoMock.Verify(r => r.ObterAleatoriasPorMateriaAsync(10, null, quantidadeEsperada), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoModuloSemQuestoes_DeveLancarSemCriarSessao()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1))
                .ReturnsAsync(new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno));
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10))
                .ReturnsAsync(new Materia("Matematica", 40, "Facil", idProfessor: 2));
            atividadeRepoMock.Setup(r => r.ObterAleatoriasPorMateriaAsync(10, 7, 5))
                .ReturnsAsync(new List<Atividade>());

            var service = new IniciarSessaoPraticaService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object, sessaoRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idMateria: 10, idModulo: 7);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Este módulo ainda não possui questões disponíveis.");

            sessaoRepoMock.Verify(r => r.AdicionarAsync(It.IsAny<SessaoPratica>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoTamanhoInvalido_DeveLancarArgumentExceptionSemConsultarNada()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();

            var service = new IniciarSessaoPraticaService(
                usuarioRepoMock.Object, new Mock<IMateriaRepository>().Object,
                new Mock<IAtividadeRepository>().Object, new Mock<ISessaoPraticaRepository>().Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idMateria: 10, tamanho: (TamanhoSessao)99);

            await act.Should().ThrowAsync<ArgumentException>();
            usuarioRepoMock.Verify(r => r.ObterPorIdAsync(It.IsAny<int>()), Times.Never);
        }

        private static Atividade ComId(Atividade atividade, int id)
        {
            typeof(Atividade).GetProperty("Id")?.SetValue(atividade, id);
            return atividade;
        }
    }
}