    using FluentAssertions;
    using Moq;
    using System;
    using System.Threading.Tasks;
    using Xunit;
    using GameficandoEstudos.Application.DTOs;
    using GameficandoEstudos.Application.Interfaces;
    using GameficandoEstudos.Application.Services.AtividadeService;
    using GameficandoEstudos.Domain.Entities;
    using GameficandoEstudos.Domain.Enums;

    namespace GameficandoEstudos.Tests.Services.AtividadeService
    {
        public class AtualizarAtividadeServiceTests
        {
            private readonly Mock<IAtividadeRepository> _atividadeRepositoryMock;
            private readonly Mock<IMateriaRepository> _materiaRepositoryMock;
            private readonly Mock<IAuditoriaService> _auditoriaServiceMock;
            private readonly AtualizarAtividadeService _service;

            public AtualizarAtividadeServiceTests()
            {
                _atividadeRepositoryMock = new Mock<IAtividadeRepository>();
                _materiaRepositoryMock = new Mock<IMateriaRepository>();
                _auditoriaServiceMock = new Mock<IAuditoriaService>();
                _service = new AtualizarAtividadeService(
                    _atividadeRepositoryMock.Object, _materiaRepositoryMock.Object, _auditoriaServiceMock.Object);
            }

            [Fact]
            public async Task ExecutarAsync_QuandoDadosForemValidos_DeveAtualizarAtividadeNoBanco()
            {
                var professorLogadoId = 1;
                var atividade = new Atividade("Tema Antigo", TipoAtividade.MultiplaEscolha, "A) 1", "A", 10);
                var materia = new Materia("Matemática", 40, "Fácil", professorLogadoId);
                var dto = new AtualizarAtividadeDto(1, "Novo Tema", "Nova Pergunta", "B");

                _atividadeRepositoryMock.Setup(r => r.ObterPorIdAsync(dto.Id)).ReturnsAsync(atividade);
                _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(atividade.IdMateria)).ReturnsAsync(materia);

                await _service.ExecutarAsync(dto, professorLogadoId);

                atividade.Nome.Should().Be("Novo Tema");
                atividade.Conteudo.Should().Be("Nova Pergunta");
                atividade.Gabarito.Should().Be("B");

                _atividadeRepositoryMock.Verify(r => r.AtualizarAsync(atividade), Times.Once);
                _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                    professorLogadoId, "Atividade.Atualizar", "Atividade", dto.Id.ToString(), It.IsAny<string>()), Times.Once);
            }

            [Fact]
            public async Task ExecutarAsync_QuandoAtividadeNaoExistir_DeveLancarInvalidOperationException()
            {
                var dto = new AtualizarAtividadeDto(999, "Tema", "Conteúdo", "A");

                _atividadeRepositoryMock.Setup(r => r.ObterPorIdAsync(dto.Id)).ReturnsAsync((Atividade)null);

                Func<Task> act = async () => await _service.ExecutarAsync(dto, 1);

                await act.Should().ThrowAsync<InvalidOperationException>()
                         .WithMessage("Atividade não encontrada.");

                _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }

            [Fact]
            public async Task ExecutarAsync_QuandoProfessorTentarAtualizarAtividadeDeOutro_DeveLancarUnauthorizedAccessException()
            {
                var dto = new AtualizarAtividadeDto(1, "Tema", "Conteúdo", "A");
                var professorInvasorId = 1;
                var professorDonoId = 2;

                var atividade = new Atividade("Tema", TipoAtividade.Dissertativa, "Pergunta", "Resposta", 10);
                var materia = new Materia("História", 60, "Média", professorDonoId);

                _atividadeRepositoryMock.Setup(r => r.ObterPorIdAsync(dto.Id)).ReturnsAsync(atividade);
                _materiaRepositoryMock.Setup(r => r.ObterPorIdAsync(atividade.IdMateria)).ReturnsAsync(materia);

                Func<Task> act = async () => await _service.ExecutarAsync(dto, professorInvasorId);

                await act.Should().ThrowAsync<UnauthorizedAccessException>()
                         .WithMessage("Você não tem permissão para alterar atividades de outros professores.");

                _atividadeRepositoryMock.Verify(r => r.AtualizarAsync(It.IsAny<Atividade>()), Times.Never);
                _auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                    It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            }
        }
    }