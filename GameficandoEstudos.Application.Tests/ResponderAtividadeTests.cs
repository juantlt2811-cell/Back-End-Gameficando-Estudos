using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests
{
    public class ResponderAtividadeTests
    {
        [Fact]
        public async Task ResponderAsync_QuandoMultiplaEscolhaCorreta_DeveDarXPeRetornarAprovado()
        {
            var llmServiceMock = new Mock<ILlmService>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            var usuarioRepositoryMock = new Mock<IUsuarioRepository>();

            int idAluno = 1;
            int idAtividade = 1;
            string respostaCorreta = "A";

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            var atividade = new Atividade("Questao 1", TipoAtividade.MultiplaEscolha, "Qual a letra?", gabarito: "A", idMateria: 1);

            usuarioRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idAluno)).ReturnsAsync(aluno);

            atividadeRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idAtividade)).ReturnsAsync(atividade);

            var service = new ResponderAtividadeService(llmServiceMock.Object, atividadeRepositoryMock.Object, usuarioRepositoryMock.Object);

            var resultado = await service.ResponderAsync(idAluno, idAtividade, respostaCorreta);

            resultado.Should().NotBeNull();

            resultado.Aprovado.Should().BeTrue();

            aluno.Gamificacao.PontosXP.Should().Be(100);

            usuarioRepositoryMock.Verify(repo => repo.AtualizarAsync(aluno), Times.Once);
        }

        [Fact]
        public async Task ResponderAsync_QuandoMultiplaEscolhaIncorreta_NaoDeveDarXPeRetornarReprovado()
        {
            // Arrange
            // TODO: Preparar os mocks
            // TODO: Criar a atividade com gabarito "A", mas passar a respostaAluno como "B"

            // Act
            // TODO: Chamar o serviço

            // Assert
            // TODO: Validar se resultado.Aprovado é falso
            // TODO: Validar se aluno.Gamificacao.PontosXP continua 0
            // TODO: Validar com o Verify se o usuarioRepositoryMock.AtualizarAsync NUNCA foi chamado (Times.Never)
            var llmServiceMock = new Mock<ILlmService>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            var usuarioRepositoryMock = new Mock<IUsuarioRepository>();

            int idAluno = 1;
            int idAtividade = 1;
            string respostaAluno = "B";

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            var atividade = new Atividade("Questao 1", TipoAtividade.MultiplaEscolha, "Qual a letra?", gabarito: "A", idMateria: 1);

            usuarioRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idAluno)).ReturnsAsync(aluno);

            atividadeRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idAtividade)).ReturnsAsync(atividade);

            var service = new ResponderAtividadeService(llmServiceMock.Object, atividadeRepositoryMock.Object, usuarioRepositoryMock.Object);

            var resultado = await service.ResponderAsync(idAluno, idAtividade, respostaAluno);


            resultado.Should().NotBeNull();
            resultado.Aprovado.Should().BeFalse();
            aluno.Gamificacao.PontosXP.Should().Be(0);
            usuarioRepositoryMock.Verify(repo => repo.AtualizarAsync(aluno), Times.Never);
        }

        [Fact]
        public async Task ResponderAsync_QuandoDissertativaCorreta_DeveAvaliarComIADarXPeRetornarAprovado()
        {
            var llmServiceMock = new Mock<ILlmService>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            var usuarioRepositoryMock = new Mock<IUsuarioRepository>();

            int idAluno = 1;
            int idAtividade = 1;
            string respostaAluno = "O loop for serve para iterar...";

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha", TipoUsuario.Aluno);
            var atividade = new Atividade("O que é um FOR?", TipoAtividade.Dissertativa, "Explique o FOR", gabarito: "Estrutura de repetição", idMateria: 1);

            // TODO: Configurar mocks de obter usuário e atividade

            usuarioRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idAluno)).ReturnsAsync(aluno);

            atividadeRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idAtividade)).ReturnsAsync(atividade);

            // TODO: Configurar llmServiceMock.AvaliarRespostaAsync para retornar um ResultadoAvaliacaoDto(true, "Ótimo!")

            llmServiceMock.Setup(service => service.AvaliarRespostaAsync(atividade.Conteudo, respostaAluno, atividade.Gabarito))
                .ReturnsAsync(new ResultadoAvaliacaoDto(true, "Ótimo!"));

            var service = new ResponderAtividadeService(llmServiceMock.Object, atividadeRepositoryMock.Object, usuarioRepositoryMock.Object);

            // Act
            var resultado = await service.ResponderAsync(idAluno, idAtividade, respostaAluno);

            // Assert
            // TODO: Validar se o resultado é Aprovado
            resultado.Aprovado.Should().BeTrue();
            // TODO: Validar se o aluno ganhou 100 de XP
            aluno.Gamificacao.PontosXP.Should().Be(100);
            // TODO: Validar se o llmServiceMock foi realmente chamado (Times.Once)
            llmServiceMock.Verify(service => service.AvaliarRespostaAsync(atividade.Conteudo, respostaAluno, atividade.Gabarito), Times.Once);
        }

        [Fact]
        public async Task ResponderAsync_QuandoUsuarioNaoForAluno_DeveLancarExcecao()
        {
            var llmServiceMock = new Mock<ILlmService>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            var usuarioRepositoryMock = new Mock<IUsuarioRepository>();

            int idUsuarioProfessor = 1;
            int idAtividade = 1;
            string respostaAluno = "A";
            // Arrange
            // TODO: Configurar um usuário com TipoUsuario.Professor
            var professor = new Usuario("Professor Teste", "professor@teste.com", "senha123", TipoUsuario.Professor);

            var atividade = new Atividade("O que é um FOR?", TipoAtividade.Dissertativa, "Explique o FOR", gabarito: "Estrutura de repetição", idMateria: 1);

            usuarioRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(() =>
                {
                    var usuarioExistente = new Usuario("Professor Teste", "professor@teste.com", "senha123", TipoUsuario.Professor);
                    typeof(Usuario).GetProperty(nameof(Usuario.Id))?.SetValue(usuarioExistente, idUsuarioProfessor);
                    return usuarioExistente;
                });
            // Act
            // TODO: Func<Task> act = async () => ...
            var service = new ResponderAtividadeService(llmServiceMock.Object, atividadeRepositoryMock.Object, usuarioRepositoryMock.Object);
            Func<Task> act = async () => await service.ResponderAsync(idUsuarioProfessor, idAtividade, respostaAluno);
            
            // Assert
            // TODO: act.Should().ThrowAsync<UnauthorizedAccessException>()
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Apenas alunos podem responder atividades.");
        }
    }
}