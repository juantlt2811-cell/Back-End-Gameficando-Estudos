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
        public async Task ResponderAsync_QuandoMultiplaEscolhaCorreta_DeveAumentarOfensivaERetornarAprovado()
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

            aluno.Gamificacao!.AtividadesFeitasHoje.Should().Be(1);
            aluno.Gamificacao.OfensivaAtual.Should().Be(1);

            usuarioRepositoryMock.Verify(repo => repo.AtualizarAsync(aluno), Times.Once);
        }

        [Fact]
        public async Task ResponderAsync_QuandoMultiplaEscolhaIncorreta_DeveAumentarOfensivaMesmoReprovado()
        {
            var llmServiceMock = new Mock<ILlmService>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            var usuarioRepositoryMock = new Mock<IUsuarioRepository>();

            int idAluno = 1;
            int idAtividade = 1;
            string respostaIncorreta = "B";

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);
            var atividade = new Atividade("Questao 1", TipoAtividade.MultiplaEscolha, "Qual a letra?", gabarito: "A", idMateria: 1);

            usuarioRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idAluno)).ReturnsAsync(aluno);
            atividadeRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idAtividade)).ReturnsAsync(atividade);

            var service = new ResponderAtividadeService(llmServiceMock.Object, atividadeRepositoryMock.Object, usuarioRepositoryMock.Object);

            var resultado = await service.ResponderAsync(idAluno, idAtividade, respostaIncorreta);

            resultado.Should().NotBeNull();
            resultado.Aprovado.Should().BeFalse();

            aluno.Gamificacao!.AtividadesFeitasHoje.Should().Be(1);

            usuarioRepositoryMock.Verify(repo => repo.AtualizarAsync(aluno), Times.Once);
        }

        [Fact]
        public async Task ResponderAsync_QuandoDissertativaCorreta_DeveAvaliarComIAEAumentarOfensiva()
        {
            var llmServiceMock = new Mock<ILlmService>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            var usuarioRepositoryMock = new Mock<IUsuarioRepository>();

            int idAluno = 1;
            int idAtividade = 1;
            string respostaAluno = "O loop for serve para iterar...";

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha", TipoUsuario.Aluno);
            var atividade = new Atividade("O que é um FOR?", TipoAtividade.Dissertativa, "Explique o FOR", gabarito: "Estrutura de repetição", idMateria: 1);

            usuarioRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idAluno)).ReturnsAsync(aluno);
            atividadeRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idAtividade)).ReturnsAsync(atividade);

            llmServiceMock.Setup(service => service.AvaliarRespostaAsync(atividade.Conteudo, respostaAluno, atividade.Gabarito))
                .ReturnsAsync(new ResultadoAvaliacaoDto(true, "Ótimo!"));

            var service = new ResponderAtividadeService(llmServiceMock.Object, atividadeRepositoryMock.Object, usuarioRepositoryMock.Object);

            var resultado = await service.ResponderAsync(idAluno, idAtividade, respostaAluno);

            resultado.Aprovado.Should().BeTrue();
            aluno.Gamificacao!.OfensivaAtual.Should().Be(1);

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

            var professor = new Usuario("Professor Teste", "professor@teste.com", "senha123", TipoUsuario.Professor);

            usuarioRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idUsuarioProfessor)).ReturnsAsync(professor);

            var service = new ResponderAtividadeService(llmServiceMock.Object, atividadeRepositoryMock.Object, usuarioRepositoryMock.Object);

            Func<Task> act = async () => await service.ResponderAsync(idUsuarioProfessor, idAtividade, respostaAluno);

            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Apenas alunos podem responder atividades.");
        }

        [Fact]
        public async Task ResponderAsync_QuandoLimiteDiarioAtingido_DeveLancarExcecaoEBloquear()
        {
            var llmServiceMock = new Mock<ILlmService>();
            var atividadeRepositoryMock = new Mock<IAtividadeRepository>();
            var usuarioRepositoryMock = new Mock<IUsuarioRepository>();

            int idAluno = 1;
            int idAtividade = 1;

            var aluno = new Usuario("Aluno Teste", "aluno@teste.com", "senha123", TipoUsuario.Aluno);

            var gamificacao = aluno.Gamificacao;
            typeof(PerfilGamificacao).GetProperty("AtividadesFeitasHoje")?.SetValue(gamificacao, 3);
            typeof(PerfilGamificacao).GetProperty("UltimaAtividadeRealizadaEm")?.SetValue(gamificacao, DateTime.UtcNow);

            usuarioRepositoryMock.Setup(repo => repo.ObterPorIdAsync(idAluno)).ReturnsAsync(aluno);

            var service = new ResponderAtividadeService(llmServiceMock.Object, atividadeRepositoryMock.Object, usuarioRepositoryMock.Object);

            Func<Task> act = async () => await service.ResponderAsync(idAluno, idAtividade, "Alguma Resposta");

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Limite diário de atividades atingido. Volte amanhã!");

            atividadeRepositoryMock.Verify(repo => repo.ObterPorIdAsync(It.IsAny<int>()), Times.Never);
            usuarioRepositoryMock.Verify(repo => repo.AtualizarAsync(It.IsAny<Usuario>()), Times.Never);
        }
    }
}