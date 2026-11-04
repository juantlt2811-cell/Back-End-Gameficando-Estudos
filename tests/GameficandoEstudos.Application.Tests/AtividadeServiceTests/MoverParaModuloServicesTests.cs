using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.AtividadeService;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.AtividadeServiceTests
{
    public class MoverParaModuloServicesTests
    {
        private const int IdMateria = 10;
        private const int IdDono = 1;
        private const int IdAtividade = 3;
        private const int IdModuloDaMateria = 7;
        private const int IdModuloDeOutraMateria = 8;

        private readonly Mock<IAtividadeRepository> _atividadeRepositoryMock;
        private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
        private readonly Mock<IModuloRepository> _moduloRepositoryMock;
        private readonly Mock<ICaixaFontesRepository> _caixaFontesRepositoryMock;
        private readonly Mock<IAuditoriaService> _auditoriaServiceMock;
        private readonly Atividade _atividade;
        private readonly MoverAtividadeDeModuloService _moverAtividadeService;
        private readonly MoverMaterialDeModuloService _moverMaterialService;

        public MoverParaModuloServicesTests()
        {
            _atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            _materiaRepositoryMock = new Mock<IMateriaRepository>();
            _moduloRepositoryMock = new Mock<IModuloRepository>();
            _caixaFontesRepositoryMock = new Mock<ICaixaFontesRepository>();
            _auditoriaServiceMock = new Mock<IAuditoriaService>();

            _atividade = new Atividade("Tema", TipoAtividade.Dissertativa, "Enunciado", "Gabarito", IdMateria);

            _atividadeRepositoryMock.Setup(r => r.ObterPorIdAsync(IdAtividade)).ReturnsAsync(_atividade);
            _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(IdMateria))
                .ReturnsAsync(new Materia("Lógica", 40, "Fácil", IdDono));
            _moduloRepositoryMock.Setup(r => r.ObterPorIdAsync(IdModuloDaMateria))
                .ReturnsAsync(new Modulo("Condicionais", IdMateria));
            _moduloRepositoryMock.Setup(r => r.ObterPorIdAsync(IdModuloDeOutraMateria))
                .ReturnsAsync(new Modulo("Outro", idMateria: 99));

            _moverAtividadeService = new MoverAtividadeDeModuloService(
                _atividadeRepositoryMock.Object, _materiaRepositoryMock.Object,
                _moduloRepositoryMock.Object, _auditoriaServiceMock.Object);
            _moverMaterialService = new MoverMaterialDeModuloService(
                _materiaRepositoryMock.Object, _moduloRepositoryMock.Object,
                _caixaFontesRepositoryMock.Object, _auditoriaServiceMock.Object);
        }

        [Fact]
        public async Task MoverAtividade_QuandoProfessorNaoEhDono_DeveLancarUnauthorizedSemAlterar()
        {
            Func<Task> act = async () => await _moverAtividadeService.ExecutarAsync(IdAtividade, IdModuloDaMateria, idProfessorLogado: 2);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _atividade.IdModulo.Should().BeNull();
            _atividadeRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Atividade>()), Times.Never);
        }

        [Fact]
        public async Task MoverAtividade_QuandoModuloEhDeOutraMateria_DeveLancarSemAlterar()
        {
            Func<Task> act = async () => await _moverAtividadeService.ExecutarAsync(IdAtividade, IdModuloDeOutraMateria, IdDono);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Módulo não encontrado nesta matéria.");
            _atividade.IdModulo.Should().BeNull();
            _atividadeRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Atividade>()), Times.Never);
        }

        [Fact]
        public async Task MoverAtividade_ParaModuloEDepoisParaSemModulo_DeveAtualizarNasDuasVezes()
        {
            await _moverAtividadeService.ExecutarAsync(IdAtividade, IdModuloDaMateria, IdDono);
            _atividade.IdModulo.Should().Be(IdModuloDaMateria);

            await _moverAtividadeService.ExecutarAsync(IdAtividade, null, IdDono);
            _atividade.IdModulo.Should().BeNull();

            _atividadeRepositoryMock.Verify(r => r.AtualizarAsync(_atividade), Times.Exactly(2));
            _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                IdDono, "Atividade.MoverModulo", "Atividade", IdAtividade.ToString(), It.IsAny<string>()), Times.Exactly(2));
        }

        [Fact]
        public async Task MoverMaterial_QuandoProfessorNaoEhDono_DeveLancarUnauthorizedSemAlterar()
        {
            Func<Task> act = async () => await _moverMaterialService.ExecutarAsync(IdMateria, Guid.NewGuid(), IdModuloDaMateria, idProfessorLogado: 2);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
            _caixaFontesRepositoryMock.Verify(
                r => r.MoverDocumentoDeModuloAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task MoverMaterial_QuandoModuloEhDeOutraMateria_DeveLancarSemAlterar()
        {
            Func<Task> act = async () => await _moverMaterialService.ExecutarAsync(IdMateria, Guid.NewGuid(), IdModuloDeOutraMateria, IdDono);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Módulo não encontrado nesta matéria.");
            _caixaFontesRepositoryMock.Verify(
                r => r.MoverDocumentoDeModuloAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<int?>()), Times.Never);
        }

        [Fact]
        public async Task MoverMaterial_QuandoDocumentoNaoExisteNaMateria_DeveLancarSemAuditar()
        {
            var documentoId = Guid.NewGuid();
            _caixaFontesRepositoryMock
                .Setup(r => r.MoverDocumentoDeModuloAsync(IdMateria, documentoId, IdModuloDaMateria))
                .ReturnsAsync(0);

            Func<Task> act = async () => await _moverMaterialService.ExecutarAsync(IdMateria, documentoId, IdModuloDaMateria, IdDono);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Material não encontrado nesta matéria.");
            _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task MoverMaterial_QuandoValido_DeveMoverTodosOsTrechosERegistrarAuditoria()
        {
            var documentoId = Guid.NewGuid();
            _caixaFontesRepositoryMock
                .Setup(r => r.MoverDocumentoDeModuloAsync(IdMateria, documentoId, IdModuloDaMateria))
                .ReturnsAsync(12);

            await _moverMaterialService.ExecutarAsync(IdMateria, documentoId, IdModuloDaMateria, IdDono);

            _caixaFontesRepositoryMock.Verify(r => r.MoverDocumentoDeModuloAsync(IdMateria, documentoId, IdModuloDaMateria), Times.Once);
            _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                IdDono, "CaixaFontes.MoverMaterial", "Materia", IdMateria.ToString(), It.IsAny<string>()), Times.Once);
        }
    }
}
