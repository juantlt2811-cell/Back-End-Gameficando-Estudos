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
        private readonly Mock<IAtividadeRepository> _atividadeRepositoryMock;
        private readonly ListarAtividadesDaMateriaService _service;

        public ListarAtividadesDaMateriaServiceTests()
        {
            _atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            _service = new ListarAtividadesDaMateriaService(_atividadeRepositoryMock.Object);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioForProfessor_DeveRetornarTodasAsAtividadesInclusivesAsInativas()
        {
            var idMateria = 10;

            var atividadeAtiva = new Atividade("Tema 1", TipoAtividade.MultiplaEscolha, "Q1", "A", idMateria);
            var atividadeInativa = new Atividade("Tema 2", TipoAtividade.Codigo, "Q2", "Código", idMateria);
            atividadeInativa.Inativar(); 

            var atividadesNoBanco = new List<Atividade> { atividadeAtiva, atividadeInativa };

            _atividadeRepositoryMock.Setup(r => r.ObterPorMateriaAsync(idMateria))
                                    .ReturnsAsync(atividadesNoBanco);

            var resultado = await _service.ExecutarAsync(idMateria, TipoUsuario.Professor);

            resultado.Should().HaveCount(2);
            resultado.Should().Contain(a => a.Ativa == true);
            resultado.Should().Contain(a => a.Ativa == false);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoUsuarioForAluno_DeveRetornarApenasAtividadesAtivas()
        {
            var idMateria = 10;

            var atividadeAtiva = new Atividade("Tema 1", TipoAtividade.MultiplaEscolha, "Q1", "A", idMateria);
            var atividadeInativa = new Atividade("Tema 2", TipoAtividade.Dissertativa, "Q2", "Resp", idMateria);
            atividadeInativa.Inativar(); 

            var atividadesNoBanco = new List<Atividade> { atividadeAtiva, atividadeInativa };

            _atividadeRepositoryMock.Setup(r => r.ObterPorMateriaAsync(idMateria))
                                    .ReturnsAsync(atividadesNoBanco);

            var resultado = await _service.ExecutarAsync(idMateria, TipoUsuario.Aluno);

            resultado.Should().HaveCount(1);
            resultado.First().Nome.Should().Be("Tema 1");
            resultado.First().Ativa.Should().BeTrue();
            resultado.Should().NotContain(a => a.Nome == "Tema 2");
        }
    }
}