using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.AtividadeService;
using GameficandoEstudos.Domain.Entities;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.AtividadeServiceTests
{
    public class RemoverMaterialEstudoServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoTudoEstiverCorreto_DeveRemoverChunksPorDocumentoId()
        {
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var caixaRepoMock = new Mock<ICaixaFontesRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            int idMateria = 1;
            int idProfessorLogado = 1;
            Guid documentoId = Guid.NewGuid();

            var materia = new Materia("Programação", 60, "Média", idProfessorLogado);
            typeof(Materia).GetProperty("Id")!.SetValue(materia, idMateria);

            var caixa = new CaixaFontes("Base", idMateria);

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);
            caixaRepoMock.Setup(repo => repo.ObterPorMateriaAsync(idMateria)).ReturnsAsync(caixa);
            caixaRepoMock
                .Setup(repo => repo.RemoverChunksPorDocumentoIdAsync(idMateria, documentoId))
                .Returns(Task.CompletedTask);

            var service = new RemoverMaterialEstudoService(
                materiaRepoMock.Object, caixaRepoMock.Object, auditoriaServiceMock.Object);

            await service.ExecutarAsync(idMateria, documentoId, idProfessorLogado);

            caixaRepoMock.Verify(repo => repo.RemoverChunksPorDocumentoIdAsync(idMateria, documentoId), Times.Once);
            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                idProfessorLogado, "CaixaFontes.RemoverMaterial", "Materia", idMateria.ToString(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMateriaNaoExistir_DeveLancarExcecao()
        {
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var caixaRepoMock = new Mock<ICaixaFontesRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            int idMateria = 99;
            int idProfessorLogado = 1;
            Guid documentoId = Guid.NewGuid();

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync((Materia?)null);

            var service = new RemoverMaterialEstudoService(
                materiaRepoMock.Object, caixaRepoMock.Object, auditoriaServiceMock.Object);

            Func<Task> act = () => service.ExecutarAsync(idMateria, documentoId, idProfessorLogado);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Matéria não encontrada.");

            caixaRepoMock.Verify(repo => repo.RemoverChunksPorDocumentoIdAsync(idMateria, It.IsAny<Guid>()), Times.Never);
            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorNaoForDono_DeveLancarExcecaoNaoAutorizado()
        {
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var caixaRepoMock = new Mock<ICaixaFontesRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            int idMateria = 1;
            int idProfessorDono = 1;
            int idProfessorNaoAutorizado = 999;
            Guid documentoId = Guid.NewGuid();

            var materia = new Materia("Programação", 60, "Média", idProfessorDono);
            typeof(Materia).GetProperty("Id")!.SetValue(materia, idMateria);

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);

            var service = new RemoverMaterialEstudoService(
                materiaRepoMock.Object, caixaRepoMock.Object, auditoriaServiceMock.Object);

            Func<Task> act = () => service.ExecutarAsync(idMateria, documentoId, idProfessorNaoAutorizado);

            await act.Should()
                .ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Você não tem permissão para remover materiais desta matéria.");

            caixaRepoMock.Verify(repo => repo.ObterPorMateriaAsync(It.IsAny<int>()), Times.Never);
            caixaRepoMock.Verify(repo => repo.RemoverChunksPorDocumentoIdAsync(idMateria, It.IsAny<Guid>()), Times.Never);
            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoMateriaNaoTiverCaixa_DeveLancarExcecao()
        {
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var caixaRepoMock = new Mock<ICaixaFontesRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            int idMateria = 1;
            int idProfessorLogado = 1;
            Guid documentoId = Guid.NewGuid();

            var materia = new Materia("Programação", 60, "Média", idProfessorLogado);
            typeof(Materia).GetProperty("Id")!.SetValue(materia, idMateria);

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);
            caixaRepoMock.Setup(repo => repo.ObterPorMateriaAsync(idMateria)).ReturnsAsync((CaixaFontes)null!);

            var service = new RemoverMaterialEstudoService(
                materiaRepoMock.Object, caixaRepoMock.Object, auditoriaServiceMock.Object);

            Func<Task> act = () => service.ExecutarAsync(idMateria, documentoId, idProfessorLogado);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Esta matéria ainda não possui uma base de conhecimento.");

            caixaRepoMock.Verify(repo => repo.RemoverChunksPorDocumentoIdAsync(idMateria, It.IsAny<Guid>()), Times.Never);
            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}