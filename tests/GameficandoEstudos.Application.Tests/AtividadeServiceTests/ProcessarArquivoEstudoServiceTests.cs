using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.AtividadeService;
using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Tests.AtividadeServiceTests
{
    public class ProcessarArquivoEstudoServiceTests
    {
        private readonly Mock<IExtratorDeTextoService> _extratorMock;
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly Mock<ICaixaFontesRepository> _caixaFontesRepositoryMock;
        private readonly Mock<ILlmService> _llmServiceMock;
        private readonly Mock<IAuditoriaService> _auditoriaServiceMock;
        private readonly ProcessarArquivoEstudoService _service;

        public ProcessarArquivoEstudoServiceTests()
        {
            _extratorMock = new Mock<IExtratorDeTextoService>();
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _caixaFontesRepositoryMock = new Mock<ICaixaFontesRepository>();
            _llmServiceMock = new Mock<ILlmService>();
            _auditoriaServiceMock = new Mock<IAuditoriaService>();

            var processarMaterialService = new ProcessarMaterialEstudoService(
                _llmServiceMock.Object,
                _materiaRepositoryMock.Object,
                _caixaFontesRepositoryMock.Object,
                _auditoriaServiceMock.Object,
                new DivisorDeTextoService(new DivisorDeTextoOptions())
            );

            _service = new ProcessarArquivoEstudoService(_extratorMock.Object, processarMaterialService);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoExtensaoNaoPermitida_DeveLancarSemChamarExtrator()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("conteudo"));

            Func<Task> act = async () =>
                await _service.ExecutarAsync(1, "virus.exe", stream, stream.Length, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Tipo de arquivo '.exe' não é permitido.");

            _extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoArquivoVazio_DeveLancar()
        {
            using var stream = new MemoryStream();

            Func<Task> act = async () =>
                await _service.ExecutarAsync(1, "material.txt", stream, tamanhoBytes: 0, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Arquivo vazio.");

            _extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoArquivoExcedeTamanhoMaximo_DeveLancar()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("conteudo"));
            const long tamanhoMaiorQue10Mb = 11 * 1024 * 1024;

            Func<Task> act = async () =>
                await _service.ExecutarAsync(1, "material.txt", stream, tamanhoMaiorQue10Mb, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Arquivo excede o tamanho máximo permitido (10 MB).");

            _extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoPdfComAssinaturaInvalida_DeveLancarSemChamarExtrator()
        {
            using var stream = new MemoryStream(new byte[] { 0x4D, 0x5A, 0x00, 0x00, 0x01, 0x02 });

            Func<Task> act = async () =>
                await _service.ExecutarAsync(1, "falso.pdf", stream, stream.Length, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("O arquivo não é um PDF válido.");

            _extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoDocxComAssinaturaInvalida_DeveLancarSemChamarExtrator()
        {
            using var stream = new MemoryStream(new byte[] { 0x25, 0x50, 0x44, 0x46 });

            Func<Task> act = async () =>
                await _service.ExecutarAsync(1, "falso.docx", stream, stream.Length, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("O arquivo não é um DOCX válido.");

            _extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoExtratorRetornaTextoVazio_DeveLancar()
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("qualquer coisa"));

            _extratorMock
                .Setup(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), "vazio.txt"))
                .ReturnsAsync("   ");

            Func<Task> act = async () =>
                await _service.ExecutarAsync(1, "vazio.txt", stream, stream.Length, idProfessorLogado: 1);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Não foi possível extrair texto deste arquivo.");

            _materiaRepositoryMock.Verify(r => r.ObterPorIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoArquivoDeTextoValido_DeveExtrairEProcessarMaterial()
        {
            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 7);
            var caixa = new CaixaFontes("Base de Conhecimento - Materia 1", idMateria: 1);
            typeof(CaixaFontes).GetProperty("Id")?.SetValue(caixa, 1);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes("ignorado, o extrator que decide o texto"));

            _extratorMock
                .Setup(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), "aula1.txt"))
                .ReturnsAsync("Primeiro paragrafo relevante.\nSegundo paragrafo relevante.");

            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(materia);
            _caixaFontesRepositoryMock.Setup(r => r.ObterPorMateriaAsync(1)).ReturnsAsync(caixa);
            _llmServiceMock.Setup(l => l.GerarEmbeddingsAsync(It.IsAny<string>())).ReturnsAsync(new float[] { 0.1f, 0.2f });

            var chunksSalvos = new List<FonteChunks>();
            _caixaFontesRepositoryMock
                .Setup(r => r.AdicionarChunksAsync(It.IsAny<IEnumerable<FonteChunks>>()))
                .Callback<IEnumerable<FonteChunks>>(chunks => chunksSalvos.AddRange(chunks))
                .Returns(Task.CompletedTask);

            await _service.ExecutarAsync(1, "aula1.txt", stream, stream.Length, idProfessorLogado: 7);

            _extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), "aula1.txt"), Times.Once);
            chunksSalvos.Should().ContainSingle();
            chunksSalvos[0].Conteudo.Should().Contain("Primeiro paragrafo")
                                    .And.Contain("Segundo paragrafo");
            _caixaFontesRepositoryMock.Verify(
                r => r.AdicionarChunksAsync(It.IsAny<IEnumerable<FonteChunks>>()), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoPdfComAssinaturaValida_DeveChamarExtratorEProcessarMaterial()
        {
            var materia = new Materia("Matematica", 40, "Facil", idProfessor: 7);
            var caixa = new CaixaFontes("Base de Conhecimento - Materia 1", idMateria: 1);
            typeof(CaixaFontes).GetProperty("Id")?.SetValue(caixa, 1);

            var bytesPdf = new byte[] { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x34 };
            using var stream = new MemoryStream(bytesPdf);

            _extratorMock
                .Setup(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), "apostila.pdf"))
                .ReturnsAsync("Conteudo extraido do PDF com mais de dez caracteres.");

            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(1)).ReturnsAsync(materia);
            _caixaFontesRepositoryMock.Setup(r => r.ObterPorMateriaAsync(1)).ReturnsAsync(caixa);
            _llmServiceMock.Setup(l => l.GerarEmbeddingsAsync(It.IsAny<string>())).ReturnsAsync(new float[] { 0.1f });

            await _service.ExecutarAsync(1, "apostila.pdf", stream, stream.Length, idProfessorLogado: 7);

            _extratorMock.Verify(e => e.ExtrairTextoAsync(It.IsAny<Stream>(), "apostila.pdf"), Times.Once);
            _caixaFontesRepositoryMock.Verify(r => r.AdicionarChunksAsync(It.IsAny<IEnumerable<FonteChunks>>()), Times.Once);
        }
    }
}