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
    public class ExcluirAtividadeServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoAtividadeNaoExiste_DeveLancarExcecao()
        {
            var repositoryMock = new Mock<IAtividadeRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();
            int idAtividade = 1;
            int idProfessorLogado = 1;
            int idMateria = 10;
            int idProfessorDono = 1;

            repositoryMock.Setup(repo => repo.ObterPorIdAsync(idAtividade))
                          .ReturnsAsync((Atividade?)null);

            var materia = new Materia("Redes", 60, "Média", idProfessorDono);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);

            var service = new ExcluirAtividadeService(repositoryMock.Object, materiaRepoMock.Object, auditoriaServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(idAtividade, idProfessorLogado);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Atividade não encontrada.");

            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoJaFoiRespondida_DeveFazerSoftDelete()
        {
            var repositoryMock = new Mock<IAtividadeRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();
            int idAtividade = 1;
            int idProfessorLogado = 1;
            int idMateria = 10;
            int idProfessorDono = 1;

            var atividade = new Atividade("Questão 1", TipoAtividade.MultiplaEscolha, "Conteúdo", "Gabarito", idMateria);
            typeof(Atividade).GetProperty("Id")?.SetValue(atividade, idAtividade);

            repositoryMock.Setup(repo => repo.ObterPorIdAsync(idAtividade)).ReturnsAsync(atividade);

            repositoryMock.Setup(repo => repo.PossuiRespostasAsync(idAtividade)).ReturnsAsync(true);

            var materia = new Materia("Redes", 60, "Média", idProfessorDono);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);

            var service = new ExcluirAtividadeService(repositoryMock.Object, materiaRepoMock.Object, auditoriaServiceMock.Object);

            await service.ExecutarAsync(idAtividade, idProfessorLogado);

            atividade.Ativa.Should().BeFalse();

            repositoryMock.Verify(repo => repo.AtualizarAsync(atividade), Times.Once);
            repositoryMock.Verify(repo => repo.RemoverAsync(It.IsAny<Atividade>()), Times.Never);

            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                idProfessorLogado, "Atividade.Inativar", "Atividade", idAtividade.ToString(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoNuncaFoiRespondida_DeveFazerHardDelete()
        {
            var repositoryMock = new Mock<IAtividadeRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();
            int idAtividade = 1;
            int idMateria = 10;
            int idProfessorDono = 1;
            int idProfessorLogado = 1;

            var atividade = new Atividade("Questão Nova", TipoAtividade.Dissertativa, "Teste", "Resposta", idMateria);
            typeof(Atividade).GetProperty("Id")?.SetValue(atividade, idAtividade);

            repositoryMock.Setup(repo => repo.ObterPorIdAsync(idAtividade)).ReturnsAsync(atividade);

            repositoryMock.Setup(repo => repo.PossuiRespostasAsync(idAtividade)).ReturnsAsync(false);

            var materia = new Materia("Redes", 60, "Média", idProfessorDono);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);

            var service = new ExcluirAtividadeService(repositoryMock.Object, materiaRepoMock.Object, auditoriaServiceMock.Object);

            await service.ExecutarAsync(idAtividade, idProfessorLogado);

            repositoryMock.Verify(repo => repo.RemoverAsync(atividade), Times.Once);
            repositoryMock.Verify(repo => repo.AtualizarAsync(It.IsAny<Atividade>()), Times.Never);

            atividade.Ativa.Should().BeTrue();

            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                idProfessorLogado, "Atividade.Remover", "Atividade", idAtividade.ToString(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoProfessorNaoForDono_DeveLancarExcecaoDeNaoAutorizado()
        {
            var atividadeRepoMock = new Mock<IAtividadeRepository>();
            var materiaRepoMock = new Mock<IMateriaRepository>();
            var auditoriaServiceMock = new Mock<IAuditoriaService>();

            int idAtividade = 1;
            int idMateria = 10;
            int idProfessorHacker = 999;
            int idProfessorDono = 1;

            var atividade = new Atividade("Questão", TipoAtividade.MultiplaEscolha, "Conteúdo", "Gabarito", idMateria);
            typeof(Atividade).GetProperty("Id")?.SetValue(atividade, idAtividade);

            var materia = new Materia("Redes", 60, "Média", idProfessorDono);
            typeof(Materia).GetProperty("Id")?.SetValue(materia, idMateria);

            atividadeRepoMock.Setup(repo => repo.ObterPorIdAsync(idAtividade)).ReturnsAsync(atividade);
            materiaRepoMock.Setup(repo => repo.ObterPorIdAsync(idMateria)).ReturnsAsync(materia);

            var service = new ExcluirAtividadeService(atividadeRepoMock.Object, materiaRepoMock.Object, auditoriaServiceMock.Object);

            Func<Task> act = async () => await service.ExecutarAsync(idAtividade, idProfessorHacker);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Você não tem permissão para alterar ou excluir atividades de outros professores.");

            atividadeRepoMock.Verify(repo => repo.RemoverAsync(It.IsAny<Atividade>()), Times.Never);
            atividadeRepoMock.Verify(repo => repo.AtualizarAsync(It.IsAny<Atividade>()), Times.Never);
            auditoriaServiceMock.Verify(a => a.RegistrarAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}