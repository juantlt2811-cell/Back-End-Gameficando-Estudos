using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Application.Services
{
    public class CriarMateriaService
    {
        private readonly IMateriaRepository _materiaRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public CriarMateriaService(IMateriaRepository materiaRepository, IUsuarioRepository usuarioRepository)
        {
            _materiaRepository = materiaRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<Materia> CriarAsync(string nome, int cargaHoraria, string dificuldade, int idUsuario)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(idUsuario);
            if(usuario == null)
                throw new InvalidOperationException("Usuário não encontrado");

            if(usuario.Tipo == TipoUsuario.Aluno)
                throw new UnauthorizedAccessException("Apenas professores ou administradores podem criar matérias.");

            var materia = new Materia(nome, cargaHoraria, dificuldade, idUsuario);

            await _materiaRepository.AdicionarAsync(materia);

            return materia;
        }
    }
}
