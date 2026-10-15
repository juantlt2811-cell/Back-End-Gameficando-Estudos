using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.AtividadeService;
using GameficandoEstudos.Domain.Entities;
using Moq;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.AtividadeServiceTests
{
    public class ProcessarArquivoEstudoServiceTests
    {
        private static (
            ProcessarArquivoEstudoService service,
            Mock<IExtratorDeTextoService> extratorMock,
            Mock<IMateriaRepository> materiaRepoMock,
            Mock<ICaixaFontesRepository> caixaRepoMock,
            Mock<ILlmService> llmMock) CriarService()
        {
            var extratorMock = new Mock<IExtratorDeTextoService>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var caixaRepoMock = new Mock<ICaixaFontesRepository>();
            var llmMock = new Mock<ILlmService>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            var processarMaterialService = new ProcessarMaterialEstudoService(
                llmMock.Object, materiaRepoMock.Object, caixaRepoMock.Object, auditoriaServiceMock.Object);

            var service = new ProcessarArquivoEstudoService(extratorMock.Object, processarMaterialService);

            return (service, extratorMock, materiaRepoMock, caixaRepoMock, llmMock);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoExtensaoNaoPermitida_DeveLancarSemChamarExtrator()
        {
            var (service, extratorMock, _, _, _) = CriarService();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("conteudo"));

            Func<Task> act = async () =>
                await service.ExecutarAsync(1, "virus.exe", stream, stream.Length, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Tipo de arquivo '.exe' não é permitido.");

            extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoArquivoVazio_DeveLancar()
        {
            var (service, extratorMock, _, _, _) = CriarService();
            using var stream = new MemoryStream();

            Func<Task> act = async () =>
                await service.ExecutarAsync(1, "material.txt", stream, tamanhoBytes: 0, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Arquivo vazio.");

            extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoArquivoExcedeTamanhoMaximo_DeveLancar()
        {
            var (service, extratorMock, _, _, _) = CriarService();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("conteudo"));
            const long tamanhoMaiorQue10Mb = 11 * 1024 * 1024;

            Func<Task> act = async () =>
                await service.ExecutarAsync(1, "material.txt", stream, tamanhoMaiorQue10Mb, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Arquivo excede o tamanho máximo permitido (10 MB).");

            extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoPdfComAssinaturaInvalida_DeveLancarSemChamarExtrator()
        {
            var (service, extratorMock, _, _, _) = CriarService();

            using var stream = new MemoryStream(new byte[] { 0x4D, 0x5A, 0x00, 0x00, 0x01, 0x02 });

            Func<Task> act = async () =>
                await service.ExecutarAsync(1, "falso.pdf", stream, stream.Length, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("O arquivo não é um PDF válido.");

            extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoDocxComAssinaturaInvalida_DeveLancarSemChamarExtrator()
        {
            var (service, extratorMock, _, _, _) = CriarService();

            using var stream = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46 }); 

            Func<Task> act = async () =>
                await service.ExecutarAsync(1, "falso.docx", stream, stream.Length, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("O arquivo não é um DOCX válido.");

            extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoExtratorRetornaTextoVazio_DeveLancar()
        {
            var (service, extratorMock, materiaRepoMock, _, _) = CriarService();
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("qualquer coisa"));

            extratorMock
                .Setup(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), "vazio.txt"))
                .ReturnsAsync("   ");

            Func<Task> act = async () =>
                await service.ExecutarAsync(1, "vazio.txt", stream, stream.Length, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Não foi possível extrair texto deste arquivo.");

            materiaRepoMock.Verify(r => r.ObterPorIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoArquivoDeTextoValido_DeveExtrairEProcessarMaterial()
        {
            var (service, extratorMock, materiaRepoMock, caixaRepoMock, llmMock) = CriarService();

            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 7);
            var caixa = new CaixaFontes("Base de Conhecimento - Materia 1", idMateria: 1);
            typeof(CaixaFontes).GetProperty("Id")?.SetValue(caixa, 1);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("ignorado, o extrator que decide o texto"));
            
            extratorMock
                .Setup(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), "aula1.txt"))
                .ReturnsAsync("Primeiro paragrafo relevante.\nSegundo paragrafo relevante.");

            materiaRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(materia);
            caixaRepoMock.Setup(r => r.ObterPorMateriaAsync(1)).ReturnsAsync(caixa);
            llmMock.Setup(l => l.GerarEmbeddingsAsync(It.IsAny<string>())).ReturnsAsync(new float[] { 0.1f, 0.2f });

            await service.ExecutarAsync(1, "aula1.txt", stream, stream.Length, idProfessorLogado: 7);

            extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), "aula1.txt"), Times.Once);
            caixa.Chunks.Should().HaveCount(2);
            caixaRepoMock.Verify(r => r.AtualizarAsync(caixa), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoPdfComAssinaturaValida_DeveChamarExtratorEProcessarMaterial()
        {
            var (service, extratorMock, materiaRepoMock, caixaRepoMock, llmMock) = CriarService();

            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 7);
            var caixa = new CaixaFontes("Base de Conhecimento - Materia 1", idMateria: 1);
            typeof(CaixaFontes).GetProperty("Id")?.SetValue(caixa, 1);

            var bytesPdf = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 }; 
            using var stream = new MemoryStream(bytesPdf);

            extratorMock
                .Setup(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), "apostila.pdf"))
                .ReturnsAsync("Conteudo extraido do PDF com mais de dez caracteres.");

            materiaRepoMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(materia);
            caixaRepoMock.Setup(r => r.ObterPorMateriaAsync(1)).ReturnsAsync(caixa);
            llmMock.Setup(l => l.GerarEmbeddingsAsync(It.IsAny<string>())).ReturnsAsync(new float[] { 0.1f });

            await service.ExecutarAsync(1, "apostila.pdf", stream, stream.Length, idProfessorLogado: 7);

            extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), "apostila.pdf"), Times.Once);
            caixaRepoMock.Verify(r => r.AtualizarAsync(caixa), Times.Once);
        }
    }
}