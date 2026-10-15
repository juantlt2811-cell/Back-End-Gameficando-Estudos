using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.UsuarioService;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.UsuarioServiceTests
{
    public class ExportarDadosUsuarioServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioNaoExiste_DeveLancarInvalidOperation()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync((Usuario?)null);

            var service = new ExportarDadosUsuarioService(
                usuarioRepoMock.Object, materiaRepoMock.Object, sessaoRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Usuário não encontrado.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessor_DeveRetornarMateriasCriadasSemSessoes()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();

            var professor = new Usuario("Professor Teste", "professor@teste.com", "hash", TipoUsuario.Professor);
            typeof(Usuario).GetProperty("Id")?.SetValue(professor, 1);

            var materias = new List<Materia>
            {
                new("Matemática", 40, "Fácil", 1),
                new("Física", 60, "Média", 1)
            };

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(professor);
            materiaRepoMock.Setup(r => r.ObterPorProfessorAsync(1)).ReturnsAsync(materias);

            var service = new ExportarDadosUsuarioService(
                usuarioRepoMock.Object, materiaRepoMock.Object, sessaoRepoMock.Object);

            var resultado = await service.ExecutarAsync(1);

            resultado.Tipo.Should().Be("Professor");
            resultado.Gamificacao.Should().BeNull(); 
            resultado.MateriasCriadas.Should().HaveCount(2);
            resultado.MateriasCriadas.Should().Contain(m => m.Nome == "Matemática");
            resultado.SessoesDePratica.Should().BeNull();

            sessaoRepoMock.Verify(r => r.ListarPorUsuarioAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoAluno_DeveRetornarSessoesDePraticaSemMaterias()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var sessaoRepoMock = new Mock<ISessaoPraticaRepository>();

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "hash", TipoUsuario.Aluno);
            typeof(Usuario).GetProperty("Id")?.SetValue(aluno, 2);

            var sessao = new SessaoPratica(idUsuario: 2, idMateria: 10, totalQuestoes: 2);
            sessao.RegistrarTentativa(new TentativaResposta(
                idSessaoPratica: 1, idAtividade: 1, idUsuario: 2, idMateria: 10,
                respostaAluno: "A", acertou: true, feedback: "Correto!"));

            usuarioRepoMock.Setup(r => r.ObterPorIdAsync(2)).ReturnsAsync(aluno);
            sessaoRepoMock.Setup(r => r.ListarPorUsuarioAsync(2)).ReturnsAsync(new List<SessaoPratica> { sessao });

            var service = new ExportarDadosUsuarioService(
                usuarioRepoMock.Object, materiaRepoMock.Object, sessaoRepoMock.Object);

            var resultado = await service.ExecutarAsync(2);

            resultado.Tipo.Should().Be("Aluno");
            resultado.Gamificacao.Should().NotBeNull();
            resultado.MateriasCriadas.Should().BeNull();
            resultado.SessoesDePratica.Should().HaveCount(1);
            resultado.SessoesDePratica![0].TotalRespondidas.Should().Be(1);
            resultado.SessoesDePratica[0].TotalAcertos.Should().Be(1);

            materiaRepoMock.Verify(r => r.ObterPorProfessorAsync(It.IsAny<int>()), Times.Never);
        }
    }
}