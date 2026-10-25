using FluentAssertions;
using Moq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.AtividadeService;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Tests.Services.AtividadeService
{
    public class ListarAtividadesDaMateriaServiceTests
    {
        private const int IdMateria = 10;
        private const int IdDono = 7;
        private const int IdOutroProfessor = 8;

        private readonly Mock<IAtividadeRepository> _atividadeRepositoryMock = new();
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock = new();
        private readonly ListarAtividadesDaMateriaService _service;

        public ListarAtividadesDaMateriaServiceTests()
        {
            _service = new ListarAtividadesDaMateriaService(
                _atividadeRepositoryMock.Object, _materiaRepositoryMock.Object);

            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(IdMateria))
                .ReturnsAsync(new Materia("Matéria", 60, "Fácil", IdDono));

            var ativa = new Atividade("Tema 1", TipoAtividade.MultiplaEscolha, "Q1", "A", IdMateria);
            var inativa = new Atividade("Tema 2", TipoAtividade.Dissertativa, "Q2", "Resp", IdMateria);
            inativa.Inativar();

            _atividadeRepositoryMock.Setup(r => r.ObterPorMateriaAsync(IdMateria))
                .ReturnsAsync(new List<Atividade> { ativa, inativa });
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorDonoDaMateria_DeveRetornarTodasInclusiveInativas()
        {
            var resultado = await _service.ExecutarAsync(IdMateria, TipoUsuario.Professor, IdDono);

            resultado.Should().HaveCount(2);
            resultado.Should().Contain(a => a.Ativa);
            resultado.Should().Contain(a => !a.Ativa);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorDeOutraMateria_DeveRetornarApenasAtivas()
        {
            var resultado = await _service.ExecutarAsync(IdMateria, TipoUsuario.Professor, IdOutroProfessor);

            resultado.Should().ContainSingle();
            resultado.Single().Nome.Should().Be("Tema 1");
            resultado.Should().NotContain(a => !a.Ativa);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoAluno_DeveRetornarApenasAtivas()
        {
            var resultado = await _service.ExecutarAsync(IdMateria, TipoUsuario.Aluno, 99);

            resultado.Should().ContainSingle();
            resultado.Single().Ativa.Should().BeTrue();
        }

        [Fact]
        public async Task ExecutarAsync_QuandoAlunoTemOMesmoIdDoDono_NaoDeveVerInativas()
        {
            var resultado = await _service.ExecutarAsync(IdMateria, TipoUsuario.Aluno, IdDono);

            resultado.Should().NotContain(a => !a.Ativa);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoAdmin_DeveRetornarTodas()
        {
            var resultado = await _service.ExecutarAsync(IdMateria, TipoUsuario.Admin, 1);

            resultado.Should().HaveCount(2);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMateriaNaoExiste_DeveRetornarListaVaziaSemBuscarAtividades()
        {
            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(999)).ReturnsAsync((Materia?)null);

            var resultado = await _service.ExecutarAsync(999, TipoUsuario.Professor, IdDono);

            resultado.Should().BeEmpty();
            _atividadeRepositoryMock.Verify(r => r.ObterPorMateriaAsync(It.IsAny<int>()), Times.Never);
        }
    }
}