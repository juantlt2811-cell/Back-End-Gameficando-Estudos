using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services;
using GameficandoEstudos.Domain.Entities;
using Moq;

namespace GameficandoEstudos.Application.Tests
{
    public class ProcessarMaterialEstudoServiceTests
    {
        [Fact]
        public async Task ProcessarTextoAsync_MateriaNaoEncontrada_DeveLancarExcecao()
        {
            int idMateriaFake = 999;
            string tituloMaterialFake = "Material de Teste";
            string textoCompletoFake = "Este é um texto de teste para processar.";

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var mockLlmService = new Mock<ILlmService>();
            var caixaFontesRepositoryMock = new Mock<ICaixaFontesRepository>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Materia?)null);

            var service = new ProcessarMaterialEstudoService(mockLlmService.Object, materiaRepositoryMock.Object, caixaFontesRepositoryMock.Object);

            Func<Task> act =async () => await service.ProcessarTextoAsync(idMateriaFake, tituloMaterialFake, textoCompletoFake);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Matéria não encontrada");

            mockLlmService.Verify(ia => ia.GerarEmbeddingsAsync(It.IsAny<string>()), Times.Never);
            caixaFontesRepositoryMock.Verify(ia => ia.AdicionarAsync(It.IsAny<CaixaFontes>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarTextoAsync_ComMateriaEncontradaECaixaInexistente_DeveCriarCaixaEProcessarTexto()
        {
            int idMateriaFake = 1;
            string tituloMaterialFake = "Material de Teste";
            string textoCompletoFake = "Este é um texto de teste para processar.";

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var mockLlmService = new Mock<ILlmService>();
            var caixaFontesRepositoryMock = new Mock<ICaixaFontesRepository>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Materia("Matéria de Teste", 1, "Baixa", 1));

            caixaFontesRepositoryMock
                .Setup(repo => repo.ObterPorMateriaAsync(It.IsAny<int>()))
                .ReturnsAsync((CaixaFontes?)null);

            caixaFontesRepositoryMock
                .Setup(repo => repo.AdicionarAsync(It.IsAny<CaixaFontes>()))
                .ReturnsAsync((CaixaFontes c) =>
                {

                    typeof(CaixaFontes).GetProperty(nameof(CaixaFontes.Id))?
                        .SetValue(c, 1);
                    return c;
                });

            mockLlmService
                .Setup(service => service.GerarEmbeddingsAsync(It.IsAny<string>()))
                .ReturnsAsync(new float[] { 0.1f, 0.2f, 0.3f });

            var service = new ProcessarMaterialEstudoService(mockLlmService.Object, materiaRepositoryMock.Object, caixaFontesRepositoryMock.Object);
            await service.ProcessarTextoAsync(idMateriaFake, tituloMaterialFake, textoCompletoFake);
            caixaFontesRepositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<CaixaFontes>()), Times.Once);
            caixaFontesRepositoryMock.Verify(repo => repo.AtualizarAsync(It.IsAny<CaixaFontes>()), Times.Once);
            mockLlmService.Verify(ia => ia.GerarEmbeddingsAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }
        [Fact]
        public async Task ProcessarTextoAsync_ComMateriaEncontradaECaixaExistente_DeveUtilizarCaixaExistenteEProcessarTexto()
        {
            int idMateriaFake = 1;
            string tituloMaterialFake = "Material de Teste";
            string textoCompletoFake = "Este é um texto de teste para processar.";

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var mockLlmService = new Mock<ILlmService>();
            var caixaFontesRepositoryMock = new Mock<ICaixaFontesRepository>();

            materiaRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new Materia("Matéria de Teste", 1, "Baixa", 1));

            caixaFontesRepositoryMock
                .Setup(repo => repo.ObterPorMateriaAsync(It.IsAny<int>()))
                .ReturnsAsync(() =>
                {
                    var caixaExistente = new CaixaFontes("Caixa de teste", idMateriaFake);
                    typeof(CaixaFontes).GetProperty(nameof(CaixaFontes.Id))?.SetValue(caixaExistente, 99);
                    return caixaExistente;
                });



            mockLlmService
                .Setup(service => service.GerarEmbeddingsAsync(It.IsAny<string>()))
                .ReturnsAsync(new float[] { 0.1f, 0.2f, 0.3f });

            var service = new ProcessarMaterialEstudoService(mockLlmService.Object, materiaRepositoryMock.Object, caixaFontesRepositoryMock.Object);
            await service.ProcessarTextoAsync(idMateriaFake, tituloMaterialFake, textoCompletoFake);
            caixaFontesRepositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<CaixaFontes>()), Times.Never);
            caixaFontesRepositoryMock.Verify(repo => repo.AtualizarAsync(It.IsAny<CaixaFontes>()), Times.Once);
            mockLlmService.Verify(ia => ia.GerarEmbeddingsAsync(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
