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

            int idMateria = 1;
            int idProfessorHacker = 999;
            int idProfessorDono = 1;

            var materia = new Materia("Redes", 60, "Média", idProfessorDono);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);

            var service = new ProcessarMaterialEstudoService(llmMock.Object, materiaRepoMock.Object, caixaRepoMock.Object);

            Func<Task> act = async () => await service.ProcessarTextoAsync(idMateria, "Livro Teste", "Conteúdo do livro...", idProfessorHacker);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Você não tem permissão para adicionar materiais nesta matéria.");

            llmMock.Verify(x => x.GerarEmbeddingsAsync(It.IsAny<string>()), Times.Never);
        }
    }
}