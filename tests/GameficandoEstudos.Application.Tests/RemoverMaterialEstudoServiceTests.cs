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
    public class RemoverMaterialEstudoServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoTudoEstiverCorreto_DeveChamarRemoverChunksDoRepositorio()
        {
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var caixaRepoMock = new Mock<ICaixaFontesRepository>();

            int idMateria = 1;
            int idProfessorLogado = 1;
            string tituloMaterial = "Apostila_Csharp.pdf";

            var materia = new Materia("Programação", 60, "Média", idProfessorLogado);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            var caixa = new CaixaFontes("Base", idMateria);
            typeof(CaixaFontes).GetProperty("Id")?.SetValue(caixa, 10);

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);
            caixaRepoMock.Setup(repo => repo.ObterPorMateriaAsync(idMateria)).ReturnsAsync(caixa);

            var service = new RemoverMaterialEstudoService(materiaRepoMock.Object, caixaRepoMock.Object);

            await service.ExecutarAsync(idMateria, tituloMaterial, idProfessorLogado);

            caixaRepoMock.Verify(repo => repo.RemoverChunksPorTituloAsync(caixa.Id, tituloMaterial), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMateriaNaoExistir_DeveLancarExcecao()
        {
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var caixaRepoMock = new Mock<ICaixaFontesRepository>();

            int idMateria = 99;
            int idProfessorLogado = 1;
            string tituloMaterial = "Apostila.pdf";

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync((Materia?)null);

            var service = new RemoverMaterialEstudoService(materiaRepoMock.Object, caixaRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(idMateria, tituloMaterial, idProfessorLogado);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Matéria não encontrada.");
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorNaoForDono_DeveLancarExcecaoNaoAutorizado()
        {
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var caixaRepoMock = new Mock<ICaixaFontesRepository>();

            int idMateria = 1;
            int idProfessorHacker = 999;
            int idProfessorDono = 1;
            string tituloMaterial = "Apostila.pdf";

            var materia = new Materia("Programação", 60, "Média", idProfessorDono);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);

            var service = new RemoverMaterialEstudoService(materiaRepoMock.Object, caixaRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(idMateria, tituloMaterial, idProfessorHacker);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Você não tem permissão para remover materiais desta matéria.");

            caixaRepoMock.Verify(repo => repo.RemoverChunksPorTituloAsync(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMateriaNaoTiverCaixa_DeveLancarExcecao()
        {
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var caixaRepoMock = new Mock<ICaixaFontesRepository>();

            int idMateria = 1;
            int idProfessorLogado = 1;
            string tituloMaterial = "Apostila.pdf";

            var materia = new Materia("Programação", 60, "Média", idProfessorLogado);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);

            caixaRepoMock.Setup(repo => repo.ObterPorMateriaAsync(idMateria)).ReturnsAsync((CaixaFontes?)null);

            var service = new RemoverMaterialEstudoService(materiaRepoMock.Object, caixaRepoMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(idMateria, tituloMaterial, idProfessorLogado);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Esta matéria ainda não possui uma base de conhecimento.");
        }
    }
}