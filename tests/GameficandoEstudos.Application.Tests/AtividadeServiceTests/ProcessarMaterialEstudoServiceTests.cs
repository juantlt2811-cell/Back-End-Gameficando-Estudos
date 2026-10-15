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
    public class ProcessarMaterialEstudoServiceTests
    {
        [Fact]
        public async Task ProcessarTextoAsync_QuandoProfessorNaoForDono_DeveLancarExcecaoDeNaoAutorizado()
        {
            var llmMock = new Mock<ILlmService>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var caixaRepoMock = new Mock<ICaixaFontesRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            int idMateria = 1;
            int idProfessorHacker = 999;
            int idProfessorDono = 1;

            var materia = new Materia("Redes", 60, "Média", idProfessorDono);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);

            var service = new ProcessarMaterialEstudoService(
                llmMock.Object, materiaRepoMock.Object, caixaRepoMock.Object, auditoriaServiceMock.Object);

            Func<Task> act = async () => await service.ProcessarTextoAsync(idMateria, "Livro Teste", "Conteúdo do livro...", idProfessorHacker);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Você não tem permissão para adicionar materiais nesta matéria.");

            llmMock.Verify(x => x.GerarEmbeddingsAsync(It.IsAny<string>()), Times.Never);
            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarTextoAsync_QuandoProfessorEhDono_DeveProcessarERegistrarAuditoria()
        {
            var llmMock = new Mock<ILlmService>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var caixaRepoMock = new Mock<ICaixaFontesRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            int idMateria = 1;
            int idProfessor = 1;

            var materia = new Materia("Redes", 60, "Média", idProfessor);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            var caixa = new CaixaFontes("Base de Conhecimento - Materia 1", idMateria);
            typeof(CaixaFontes).GetProperty("Id")?.SetValue(caixa, 1);

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);
            caixaRepoMock.Setup(repo => repo.ObterPorMateriaAsync(idMateria)).ReturnsAsync(caixa);
            llmMock.Setup(l => l.GerarEmbeddingsAsync(It.IsAny<string>())).ReturnsAsync(new float[] { 0.1f });

            var service = new ProcessarMaterialEstudoService(
                llmMock.Object, materiaRepoMock.Object, caixaRepoMock.Object, auditoriaServiceMock.Object);

            await service.ProcessarTextoAsync(idMateria, "Livro Teste", "Conteúdo relevante do livro.", idProfessor);

            caixaRepoMock.Verify(repo => repo.AtualizarAsync(caixa), Times.Once);
            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                idProfessor, "CaixaFontes.AdicionarMaterial", "Materia", idMateria.ToString(), It.IsAny<string>()), Times.Once);
        }
    }
}