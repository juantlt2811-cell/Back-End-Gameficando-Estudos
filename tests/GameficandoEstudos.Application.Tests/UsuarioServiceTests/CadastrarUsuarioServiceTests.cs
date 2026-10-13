using FluentAssertions;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services.UsuarioService;
using GameficandoEstudos.Application.Validacoes;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace GameficandoEstudos.Application.Tests.UsuarioServiceTests
{
    public class CadastrarUsuarioServiceTests
    {
        [Fact]
        public async Task ExecutarAsync_QuandoEmailJaExiste_DeveLancarExcecao()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var politicaSenhaValidator = new PoliticaSenhaValidator();

            var dto = new RegistrarUsuarioDto(
                "Professor Teste",
                "professor@email.com",
                "SenhaAbc123",
                TipoUsuario.Professor);

            usuarioRepoMock
                .Setup(repo => repo.ExisteEmailAsync(dto.Email))
                .ReturnsAsync(true);

            var service = new CadastrarUsuarioService(
                usuarioRepoMock.Object,
                passwordServiceMock.Object,
                politicaSenhaValidator);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Este e-mail já está em uso.");

            usuarioRepoMock.Verify(
                repo => repo.AdicionarAsync(It.IsAny<Usuario>()),
                Times.Never);

            passwordServiceMock.Verify(
                p => p.HashPassword(It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task ExecutarAsync_QuandoDadosValidos_DeveCriptografarSenhaESalvar()
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var politicaSenhaValidator = new PoliticaSenhaValidator();

            var dto = new RegistrarUsuarioDto(
                "Novo Aluno",
                "aluno@email.com",
                "SenhaAbc123",
                TipoUsuario.Aluno);

            usuarioRepoMock
                .Setup(repo => repo.ExisteEmailAsync(dto.Email))
                .ReturnsAsync(false);

            passwordServiceMock
                .Setup(p => p.HashPassword(dto.Senha))
                .Returns("$argon2id$v=19$hash_fake");

            var service = new CadastrarUsuarioService(
                usuarioRepoMock.Object,
                passwordServiceMock.Object,
                politicaSenhaValidator);

            await service.ExecutarAsync(dto);

            passwordServiceMock.Verify(
                p => p.HashPassword(dto.Senha),
                Times.Once);

            usuarioRepoMock.Verify(repo => repo.AdicionarAsync(It.Is<Usuario>(u =>
                u.Nome == dto.Nome &&
                u.Email == dto.Email &&
                u.Senha == "$argon2id$v=19$hash_fake" &&
                u.Tipo == TipoUsuario.Aluno
            )), Times.Once);
        }

        [Theory]
        [InlineData("senhaabc123")]
        [InlineData("SENHAABC123")]
        [InlineData("SenhaAbcdef")]
        [InlineData("Ab1")]
        public async Task ExecutarAsync_QuandoSenhaInvalida_DeveLancarExcecao(string senha)
        {
            var usuarioRepoMock = new Mock<IUsuarioRepository>();
            var passwordServiceMock = new Mock<IPasswordService>();
            var politicaSenhaValidator = new PoliticaSenhaValidator();

            var dto = new RegistrarUsuarioDto(
                "Novo Aluno",
                "aluno@email.com",
                senha,
                TipoUsuario.Aluno);

            usuarioRepoMock
                .Setup(repo => repo.ExisteEmailAsync(dto.Email))
                .ReturnsAsync(false);

            var service = new CadastrarUsuarioService(
                usuarioRepoMock.Object,
                passwordServiceMock.Object,
                politicaSenhaValidator);

            Func<Task> act = async () => await service.ExecutarAsync(dto);

            await act.Should()
                .ThrowAsync<InvalidOperationException>();

            passwordServiceMock.Verify(
                p => p.HashPassword(It.IsAny<string>()),
                Times.Never);

            usuarioRepoMock.Verify(
                repo => repo.AdicionarAsync(It.IsAny<Usuario>()),
                Times.Never);
        }
    }
}
