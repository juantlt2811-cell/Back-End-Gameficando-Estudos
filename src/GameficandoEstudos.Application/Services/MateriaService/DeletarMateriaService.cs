using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.DTOs;

namespace GameficandoEstudos.Application.Services.MateriaService
{
    public class DeletarMateriaService
    {
        private readonly IMateriaRepository _materiaRepository;

        public DeletarMateriaService(IMateriaRepository materiaRepository)
        {
            _materiaRepository = materiaRepository;
        }

        public async Task ExecutarAsync(int id, int idProfessor)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(id);

            if (materia == null || materia.IdProfessor != idProfessor)
                throw new UnauthorizedAccessException("Matéria não encontrada ou você não tem permissão para excluí-la.");


            await _materiaRepository.DeletarAsync(id);
        }
    }
}