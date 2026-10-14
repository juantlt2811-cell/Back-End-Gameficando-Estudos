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
    public class VerificarPodeIniciarSessaoServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioNaoEhAluno_DeveLancarUnauthorized()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();

            var professor = new Usuario("Professor Teste", "professor@teste.com", "senha123", TipoUsuario.Professor);
            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(professor);

            var service = new VerificarPodeIniciarSessaoService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idMateria: 10);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Apenas alunos podem praticar matérias.");

            materiaRepoMock.Verify(r => r.ObterPorIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMateriaNaoExiste_DeveLancarInvalidOperation()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync((Materia?)null);

            var service = new VerificarPodeIniciarSessaoService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1, idMateria: 10);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Matéria não encontrada.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoSemQuestoesDisponiveis_DeveRetornarPodeIniciarFalso()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 2);

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(materia);
            atividadeRepoMock.Setup(r => r.ContarAtivasPorMateriaAsync(10)).ReturnsAsync(0);

            var service = new VerificarPodeIniciarSessaoService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object);

            var resultado = await service.ExecutarAsync(1, idMateria: 10);

            resultado.PodeIniciar.Should().BeFalse();
            resultado.Motivo.Should().Be("Esta matéria ainda não possui questões disponíveis.");
            resultado.TotalQuestoesDisponiveis.Should().Be(0);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoLimiteDiarioAtingido_DeveRetornarPodeIniciarFalso()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            typeof(PerfilGamificacao).GetProperty("AtividadesFeitasHoje")?.SetValue(aluno.Gamificacao, 3);
            typeof(PerfilGamificacao).GetProperty("UltimaAtividadeRealizadaEm")?.SetValue(aluno.Gamificacao, DateTime.UtcNow);

            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 2);

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(materia);
            atividadeRepoMock.Setup(r => r.ContarAtivasPorMateriaAsync(10)).ReturnsAsync(5);

            var service = new VerificarPodeIniciarSessaoService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object);

            var resultado = await service.ExecutarAsync(1, idMateria: 10);

            resultado.PodeIniciar.Should().BeFalse();
            resultado.Motivo.Should().Be("Limite diário de matérias praticadas atingido. Volte amanhã!");
            resultado.TentativasRestantesHoje.Should().Be(0);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoTudoOk_DeveRetornarPodeIniciarTrueComDadosCorretos()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            typeof(PerfilGamificacao).GetProperty("AtividadesFeitasHoje")?.SetValue(aluno.Gamificacao, 1);
            typeof(PerfilGamificacao).GetProperty("UltimaAtividadeRealizadaEm")?.SetValue(aluno.Gamificacao, DateTime.UtcNow);

            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 2);

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(materia);
            atividadeRepoMock.Setup(r => r.ContarAtivasPorMateriaAsync(10)).ReturnsAsync(8); 

            var service = new VerificarPodeIniciarSessaoService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object);

            var resultado = await service.ExecutarAsync(1, idMateria: 10);

            resultado.PodeIniciar.Should().BeTrue();
            resultado.Motivo.Should().BeNull();
            resultado.TentativasRestantesHoje.Should().Be(2); 
            resultado.TotalQuestoesDisponiveis.Should().Be(5); 
        }

        [Fact]
        public async Task ExecutarAsync_QuandoUltimaAtividadeFoiOntem_TentativasRestantesDeveVoltarAoLimiteTotal()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var atividadeRepoMock = new Mock<IAtividadeRepository>();

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            typeof(PerfilGamificacao).GetProperty("AtividadesFeitasHoje")?.SetValue(aluno.Gamificacao, 3);
            typeof(PerfilGamificacao).GetProperty("UltimaAtividadeRealizadaEm")?.SetValue(aluno.Gamificacao, DateTime.UtcNow.AddDays(-1));

            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 2);

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(aluno);
            materiaRepoMock.Setup(r => r.ObterPorIdAsync(10)).ReturnsAsync(materia);
            atividadeRepoMock.Setup(r => r.ContarAtivasPorMateriaAsync(10)).ReturnsAsync(3);

            var service = new VerificarPodeIniciarSessaoService(
                usuarioRepoMock.Object, materiaRepoMock.Object, atividadeRepoMock.Object);

            var resultado = await service.ExecutarAsync(1, idMateria: 10);

            resultado.PodeIniciar.Should().BeTrue();
            resultado.TentativasRestantesHoje.Should().Be(3); 
        }
    }
}