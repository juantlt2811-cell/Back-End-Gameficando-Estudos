using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.AtividadeService;
using GameficandoEstudos.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.AtividadeServiceTests
{
    public class ProcessarMaterialEstudoServiceTests
    {
        private readonly Mock<ILlmService> _llmServiceMock;
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly Mock<ICaixaFontesRepository> _caixaFontesRepositoryMock;
        private readonly Mock<IAuditoriaService> _auditoriaServiceMock;
        private readonly Mock<IDivisorDeTextoService> _divisorDeTextoServiceMock;
        private readonly ProcessarMaterialEstudoService _service;

        public ProcessarMaterialEstudoServiceTests()
        {
            _llmServiceMock = new Mock<ILlmService>();
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _caixaFontesRepositoryMock = new Mock<ICaixaFontesRepository>();
            _auditoriaServiceMock = new Mock<IAuditoriaService>();
            _divisorDeTextoServiceMock = new Mock<IDivisorDeTextoService>();

            _service = new ProcessarMaterialEstudoService(
                _llmServiceMock.Object,
                _materiaRepositoryMock.Object,
                _caixaFontesRepositoryMock.Object,
                _auditoriaServiceMock.Object,
                _divisorDeTextoServiceMock.Object
            );
        }

        [Fact]
        public async Task ProcessarTextoAsync_QuandoMateriaNaoExiste_DeveLancarExcecao()
        {
            var idMateria = 1;
            var idProfessor = 1;

            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(idMateria)).ReturnsAsync((Materia?)null);

            Func<Task> act = async () =>
                await _service.ProcessarTextoAsync(idMateria, "Livro Teste", "Conteúdo do livro.", idProfessor);

            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("Matéria não encontrada.");

            _divisorDeTextoServiceMock.Verify(d => d.Dividir(It.IsAny<string>()), Times.Never);
            _caixaFontesRepositoryMock.Verify(r => r.AdicionarChunksAsync(It.IsAny<IEnumerable<FonteChunks>>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarTextoAsync_QuandoProfessorNaoForDono_DeveLancarExcecaoDeNaoAutorizado()
        {
            var idMateria = 1;
            var idProfessorDono = 1;
            var idProfessorHacker = 999;

            var materia = new Materia("Redes", 60, "Média", idProfessorDono);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);

            Func<Task> act = async () =>
                await _service.ProcessarTextoAsync(idMateria, "Livro Teste", "Conteúdo do livro.", idProfessorHacker);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                     .WithMessage("Você não tem permissão para adicionar materiais nesta matéria.");

            _divisorDeTextoServiceMock.Verify(d => d.Dividir(It.IsAny<string>()), Times.Never);
            _caixaFontesRepositoryMock.Verify(r => r.AdicionarChunksAsync(It.IsAny<IEnumerable<FonteChunks>>()), Times.Never);
            _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ProcessarTextoAsync_QuandoProfessorEhDono_DeveDividirOTextoCriarChunksEembeddingsERegistrarAuditoria()
        {
            var idMateria = 1;
            var idProfessor = 1;
            var idCaixa = 7;
            var tituloMaterial = "Apostila de Redes";
            var textoCompleto = "Conteúdo relevante do livro.";

            var materia = new Materia("Redes", 60, "Média", idProfessor);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            var caixa = new CaixaFontes("Base de Conhecimento - Matéria 1", idMateria);
            typeof(CaixaFontes).GetProperty("Id")?.SetValue(caixa, idCaixa);

            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);
            _caixaFontesRepositoryMock.Setup(r => r.ObterPorMateriaAsync(idMateria)).ReturnsAsync(caixa);
            _divisorDeTextoServiceMock
                .Setup(d => d.Dividir(textoCompleto))
                .Returns(new[] { "Primeiro trecho.", "Segundo trecho.", "Terceiro trecho." });
            _llmServiceMock.Setup(l => l.GerarEmbeddingsAsync(It.IsAny<string>())).ReturnsAsync(new float[] { 0.1f });

            var chunksSalvos = new List<FonteChunks>();
            _caixaFontesRepositoryMock
                .Setup(r => r.AdicionarChunksAsync(It.IsAny<IEnumerable<FonteChunks>>()))
                .Callback<IEnumerable<FonteChunks>>(chunks => chunksSalvos.AddRange(chunks))
                .Returns(Task.CompletedTask);

            await _service.ProcessarTextoAsync(idMateria, tituloMaterial, textoCompleto, idProfessor);

            _divisorDeTextoServiceMock.Verify(d => d.Dividir(textoCompleto), Times.Once);

            chunksSalvos.Select(c => c.Conteudo).Should().Equal(
                "Primeiro trecho.", "Segundo trecho.", "Terceiro trecho.");
            chunksSalvos.Should().OnlyContain(c => c.IdCaixaFonte == idCaixa);
            chunksSalvos.Should().OnlyContain(c => c.Origem == tituloMaterial);
            chunksSalvos.Select(c => c.DocumentoId).Distinct().Should().HaveCount(1);

            _llmServiceMock.Verify(l => l.GerarEmbeddingsAsync("Primeiro trecho."), Times.Once);
            _llmServiceMock.Verify(l => l.GerarEmbeddingsAsync("Segundo trecho."), Times.Once);
            _llmServiceMock.Verify(l => l.GerarEmbeddingsAsync("Terceiro trecho."), Times.Once);

            _caixaFontesRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<CaixaFontes>()), Times.Never);
            _caixaFontesRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<CaixaFontes>()), Times.Never);

            _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                idProfessor, "CaixaFontes.AdicionarMaterial", "Materia", idMateria.ToString(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessarTextoAsync_QuandoDivisorNaoProduzTrechos_NaoDeveSalvarChunks()
        {
            var idMateria = 1;
            var idProfessor = 1;
            var idCaixa = 7;

            var materia = new Materia("Redes", 60, "Média", idProfessor);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            var caixa = new CaixaFontes("Base de Conhecimento - Matéria 1", idMateria);
            typeof(CaixaFontes).GetProperty("Id")?.SetValue(caixa, idCaixa);

            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);
            _caixaFontesRepositoryMock.Setup(r => r.ObterPorMateriaAsync(idMateria)).ReturnsAsync(caixa);
            _divisorDeTextoServiceMock.Setup(d => d.Dividir(It.IsAny<string>())).Returns(Array.Empty<string>());

            await _service.ProcessarTextoAsync(idMateria, "Livro Teste", "   ", idProfessor);

            _caixaFontesRepositoryMock.Verify(r => r.AdicionarChunksAsync(It.IsAny<IEnumerable<FonteChunks>>()), Times.Never);
            _llmServiceMock.Verify(l => l.GerarEmbeddingsAsync(It.IsAny<string>()), Times.Never);
            _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                idProfessor, "CaixaFontes.AdicionarMaterial", "Materia", idMateria.ToString(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ProcessarTextoAsync_QuandoMateriaNaoTemCaixa_DeveCriarUmaEUsarOIdGerado()
        {
            var idMateria = 1;
            var idProfessor = 1;
            var idCaixa = 7;

            var materia = new Materia("Redes", 60, "Média", idProfessor);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            var caixaPersistida = new CaixaFontes("Base de Conhecimento - Matéria 1", idMateria);
            typeof(CaixaFontes).GetProperty("Id")?.SetValue(caixaPersistida, idCaixa);

            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);
            _caixaFontesRepositoryMock.Setup(r => r.ObterPorMateriaAsync(idMateria)).ReturnsAsync((CaixaFontes)null!);
            _caixaFontesRepositoryMock.Setup(r => r.AdicionarAsync(It.IsAny<CaixaFontes>())).ReturnsAsync(caixaPersistida);
            _divisorDeTextoServiceMock.Setup(d => d.Dividir(It.IsAny<string>())).Returns(new[] { "Trecho único do material." });
            _llmServiceMock.Setup(l => l.GerarEmbeddingsAsync(It.IsAny<string>())).ReturnsAsync(new float[] { 0.1f });

            var chunksSalvos = new List<FonteChunks>();
            _caixaFontesRepositoryMock
                .Setup(r => r.AdicionarChunksAsync(It.IsAny<IEnumerable<FonteChunks>>()))
                .Callback<IEnumerable<FonteChunks>>(chunks => chunksSalvos.AddRange(chunks))
                .Returns(Task.CompletedTask);

            await _service.ProcessarTextoAsync(idMateria, "Livro Teste", "Conteúdo relevante do livro.", idProfessor);

            _caixaFontesRepositoryMock.Verify(r => r.AdicionarAsync(It.IsAny<CaixaFontes>()), Times.Once);
            chunksSalvos.Should().OnlyContain(c => c.IdCaixaFonte == idCaixa);
        }
    }
}