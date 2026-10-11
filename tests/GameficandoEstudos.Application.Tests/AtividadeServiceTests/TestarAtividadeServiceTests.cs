using FluentAssertions;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.AtividadeService;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Tests.Services.AtividadeService
{
    public class TestarAtividadeServiceTests
    {
        private readonly Mock<ILlmService> _llmServiceMock;
        private readonly Mock<IAtividadeRepository> _atividadeRepositoryMock;
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly Mock<IUsuarioRepository> _usuarioRepositoryMock;
        private readonly TestarAtividadeService _service;

        public TestarAtividadeServiceTests()
        {
            _llmServiceMock = new Mock<ILlmService>();
            _atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _usuarioRepositoryMock = new Mock<IUsuarioRepository>();

            _service = new TestarAtividadeService(
                _llmServiceMock.Object,
                _atividadeRepositoryMock.Object,
                _materiaRepositoryMock.Object,
                _usuarioRepositoryMock.Object
            );
        }

        [Fact]
        public async Task ExecutarAsync_QuandoRespostaVazia_DeveLancarArgumentException()
        {
            
            Func<Task> act = async () => await _service.ExecutarAsync(1, 1, "   ");

            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*resposta de teste não pode ser vazia*");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioNaoForProfessor_DeveLancarUnauthorizedAccessException()
        {
            var aluno = CriarUsuarioFake(TipoUsuario.Aluno);
            _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(aluno.Id)).ReturnsAsync(aluno);

            Func<Task> act = async () => await _service.ExecutarAsync(aluno.Id, 1, "Resposta teste");

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("Apenas professores podem acessar a área de testes de atividades.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorTentarTestarAtividadeDeOutraMateria_DeveLancarUnauthorizedAccessException()
        {
            var professorId = 1;
            var professorDonoId = 2;
            var professorLogado = CriarUsuarioFake(TipoUsuario.Professor, professorId);

            var atividade = new Atividade("Tema", TipoAtividade.MultiplaEscolha, "A) 1", "A", 10);
            var materiaDeOutro = new Materia("Matemática", 40, "Fácil", professorDonoId);

            _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(professorId)).ReturnsAsync(professorLogado);
            _atividadeRepositoryMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(atividade);
            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(atividade.IdMateria)).ReturnsAsync(materiaDeOutro);

            Func<Task> act = async () => await _service.ExecutarAsync(professorId, 1, "A");

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("Você não tem permissão para testar atividades de outras matérias.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMultiplaEscolhaCorreta_DeveRetornarFeedbackDeTesteCorreto()
        {
            var professorId = 1;
            var professor = CriarUsuarioFake(TipoUsuario.Professor, professorId);

            var atividade = new Atividade("Tema", TipoAtividade.MultiplaEscolha, "Pergunta", "B", 10);
            var materia = new Materia("Matemática", 40, "Fácil", professorId);

            _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(professorId)).ReturnsAsync(professor);
            _atividadeRepositoryMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(atividade);
            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(atividade.IdMateria)).ReturnsAsync(materia);

            var resultado = await _service.ExecutarAsync(professorId, 1, " b ");

            resultado.Aprovado.Should().BeTrue();
            resultado.Feedback.Should().Contain("[MODO TESTE]");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoDissertativa_DeveAvaliarComLlmERetornarFeedback()
        {
            var professorId = 1;
            var professor = CriarUsuarioFake(TipoUsuario.Professor, professorId);

            var atividade = new Atividade("Tema", TipoAtividade.Dissertativa, "Explique DDD", "Domain Driven Design...", 10);
            var materia = new Materia("Software", 40, "Média", professorId);
            var respostaAvaliacaoIa = new ResultadoAvaliacaoDto(true, "Boa explicação");

            _usuarioRepositoryMock.Setup(r => r.ObterPorIdAsync(professorId)).ReturnsAsync(professor);
            _atividadeRepositoryMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(atividade);
            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(atividade.IdMateria)).ReturnsAsync(materia);

            _llmServiceMock.Setup(l => l.AvaliarRespostaAsync(atividade.Conteudo, "Minha resposta", atividade.Gabarito))
                           .ReturnsAsync(respostaAvaliacaoIa);

            var resultado = await _service.ExecutarAsync(professorId, 1, "Minha resposta");

            resultado.Aprovado.Should().BeTrue();
            resultado.Feedback.Should().Be("[MODO TESTE] Boa explicação");
            _llmServiceMock.Verify(l => l.AvaliarRespostaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        private Usuario CriarUsuarioFake(TipoUsuario tipo, int id = 1)
        {
            var usuario = (Usuario)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Usuario));
            typeof(Usuario).GetProperty("Id")?.SetValue(usuario, id);
            typeof(Usuario).GetProperty("Tipo")?.SetValue(usuario, tipo);
            return usuario;
        }
    }
}