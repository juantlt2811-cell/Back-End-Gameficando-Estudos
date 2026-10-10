using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services;
using GameficandoEstudos.Domain.Entities;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests
{
    public class BuscarMateriaServiceTests
    {
        [Fact]
        public async Task ObterPorIdAsync_QuandoMateriaExiste_DeveRetornarMateriaDto()
        {
            int idBusca = 1;
            var repositoryMock = new Mock<IMateriaRepository>();

            var materiaFake = new Materia("Redes de Computadores", 60, "Alta", 1);

            typeof(Materia).GetProperty("Id")?.SetValue(materiaFake, idBusca);
            typeof(Materia).GetProperty("CriadoEm")?.SetValue(materiaFake, DateTime.UtcNow);

            repositoryMock.Setup(repo => repo.ObterPorIdAsync(idBusca)).ReturnsAsync(materiaFake);

            var service = new BuscarMateriaService(repositoryMock.Object);

            var resultado = await service.ObterPorIdAsync(idBusca);

            resultado.Should().NotBeNull();
            resultado.Id.Should().Be(idBusca);
            resultado.Nome.Should().Be("Redes de Computadores");
            resultado.CargaHoraria.Should().Be(60);
            repositoryMock.Verify(repo => repo.ObterPorIdAsync(idBusca), Times.Once);
        }

        [Fact]
        public async Task ObterPorIdAsync_QuandoMateriaNaoExiste_DeveLancarExcecao()
        {
            int idBusca = 999;
            var repositoryMock = new Mock<IMateriaRepository>();

            repositoryMock.Setup(repo => repo.ObterPorIdAsync(idBusca)).ReturnsAsync((Materia?)null);

            var service = new BuscarMateriaService(repositoryMock.Object);

            Func<Task> act = async () => await service.ObterPorIdAsync(idBusca);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Matéria não encontrada.");
        }
        [Fact]
        public async Task PesquisarAsync_ComFiltros_DeveRetornarListaDeMateriaDto()
        {
            var repositoryMock = new Mock<IMateriaRepository>();
            string nomeFiltro = "Redes";
            int idProfessorFiltro = 1;

            var materiaFake1 = new Materia("Redes de Computadores", 60, "Alta", 1);
            typeof(Materia).GetProperty("Id")?.SetValue(materiaFake1, 1);

            var materiaFake2 = new Materia("Redes Sem Fio", 40, "Média", 1);
            typeof(Materia).GetProperty("Id")?.SetValue(materiaFake2, 2);

            var listaFake = new List<Materia> { materiaFake1, materiaFake2 };

            repositoryMock.Setup(repo => repo.PesquisarAsync(nomeFiltro, idProfessorFiltro))
                          .ReturnsAsync(listaFake);

            var service = new BuscarMateriaService(repositoryMock.Object);

            var resultado = await service.PesquisarAsync(nomeFiltro, idProfessorFiltro);

            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2);
            resultado.Should().Contain(m => m.Nome == "Redes de Computadores");
            repositoryMock.Verify(repo => repo.PesquisarAsync(nomeFiltro, idProfessorFiltro), Times.Once);
        }

        [Fact]
        public async Task PesquisarAsync_SemResultados_DeveRetornarListaVazia()
        {
            var repositoryMock = new Mock<IMateriaRepository>();

            repositoryMock.Setup(repo => repo.PesquisarAsync(It.IsAny<string>(), It.IsAny<int>()))
                          .ReturnsAsync(new List<Materia>());

            var service = new BuscarMateriaService(repositoryMock.Object);

            var resultado = await service.PesquisarAsync("Materia Inexistente", 99);

            resultado.Should().NotBeNull();
            resultado.Should().BeEmpty(); 
        }
        [Fact]
        public async Task ObterMateriasDoProfessorAsync_DeveRetornarMateriasDoProfessorEspecifico()
        {
            var repositoryMock = new Mock<IMateriaRepository>();
            int idProfessorLogado = 1;

            var materiaFake1 = new Materia("Banco de Dados", 80, "Média", idProfessorLogado);
            typeof(Materia).GetProperty("Id")?.SetValue(materiaFake1, 1);
            typeof(Materia).GetProperty("CriadoEm")?.SetValue(materiaFake1, DateTime.UtcNow);

            var materiaFake2 = new Materia("Engenharia de Software", 60, "Alta", idProfessorLogado);
            typeof(Materia).GetProperty("Id")?.SetValue(materiaFake2, 2);
            typeof(Materia).GetProperty("CriadoEm")?.SetValue(materiaFake2, DateTime.UtcNow);

            var listaFake = new List<Materia> { materiaFake1, materiaFake2 };

            repositoryMock.Setup(repo => repo.ObterPorProfessorAsync(idProfessorLogado))
                          .ReturnsAsync(listaFake);

            var service = new BuscarMateriaService(repositoryMock.Object);

            var resultado = await service.ObterMateriasDoProfessorAsync(idProfessorLogado);

            resultado.Should().NotBeNull();
            resultado.Should().HaveCount(2);
            resultado.Should().Contain(m => m.Nome == "Banco de Dados");

            repositoryMock.Verify(repo => repo.ObterPorProfessorAsync(idProfessorLogado), Times.Once);
        }
    }
}