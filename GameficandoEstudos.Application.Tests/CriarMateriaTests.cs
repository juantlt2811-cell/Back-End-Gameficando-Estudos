using FluentAssertions;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Services;
using GameficandoEstudos.Domain.Entities;
using Moq;

namespace GameficandoEstudos.Application.Tests
{
    public class CriarMateriaTests
    {
        [Fact]
        public async Task CriarMateriaAsync_DeveRetornarMateriaCriada()
        {
            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var usuarioRepositoryMock = new Mock<IUsuarioRepository>();

            string nomeMateria = "Matemática";
            int cargaHoraria = 40;
            string dificuldade = "Média";
            int idProfessor = 1;

            usuarioRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(() =>
                {
                    var usuarioExistente = new Usuario("Professor Teste", "professor@teste.com", "senhaTeste", TipoUsuario.Professor);
                    typeof(Usuario).GetProperty(nameof(Usuario.Id))?.SetValue(usuarioExistente, idProfessor);
                    return usuarioExistente;
                });

            materiaRepositoryMock
                .Setup(repo => repo.AdicionarAsync(It.IsAny<Materia>()))
                .ReturnsAsync((Materia materia) => materia);

            var service = new CriarMateriaService(materiaRepositoryMock.Object, usuarioRepositoryMock.Object);
            
            var resultado = await service.CriarAsync(nomeMateria, cargaHoraria, dificuldade, idProfessor);
            
            resultado.Should().NotBeNull();
            resultado.Nome.Should().Be("Matemática");

            materiaRepositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Materia>()), Times.Once);
        }
        [Fact]
        public async Task CriarMateriaAsync_QuandoUsuarioNaoEncontrado_RetornarExcecao()
        {
            int idUsuarioInexistente = 999;
            string nomeMateria = "Matemática";
            int cargaHoraria = 40;
            string dificuldade = "Média";

            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var usuarioRepositoryMock = new Mock<IUsuarioRepository>();

            usuarioRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync((Usuario?)null);

            var service = new CriarMateriaService(materiaRepositoryMock.Object, usuarioRepositoryMock.Object);

            Func<Task> act = async () => await service.CriarAsync(nomeMateria, cargaHoraria, dificuldade, idUsuarioInexistente);
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Usuário não encontrado");

            materiaRepositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Materia>()), Times.Never);
        }
        [Fact]
        public async Task CriarMateriaAsync_QuandoUsuarioForAluno_RetornarExcecao()
        {
            int idUsuarioAluno = 2;
            string nomeMateria = "Matemática";
            int cargaHoraria = 40;
            string dificuldade = "Média";


            var materiaRepositoryMock = new Mock<IMateriaRepository>();
            var usuarioRepositoryMock = new Mock<IUsuarioRepository>();


            usuarioRepositoryMock
                .Setup(repo => repo.ObterPorIdAsync(It.IsAny<int>()))
                .ReturnsAsync(() =>
                {
                    var usuarioExistente = new Usuario("Aluno Teste", "aluno@teste.com", "senhaTeste", TipoUsuario.Aluno);
                    typeof(Usuario).GetProperty(nameof(Usuario.Id))?.SetValue(usuarioExistente, idUsuarioAluno);
                    return usuarioExistente;
                });

            var service = new CriarMateriaService(materiaRepositoryMock.Object, usuarioRepositoryMock.Object);

            Func<Task> act = async () => await service.CriarAsync(nomeMateria, cargaHoraria, dificuldade, idUsuarioAluno);
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("Apenas professores ou administradores podem criar matérias.");

            materiaRepositoryMock.Verify(repo => repo.AdicionarAsync(It.IsAny<Materia>()), Times.Never);
        }
    }
}
