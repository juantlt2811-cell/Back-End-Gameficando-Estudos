using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.DTOs;

namespace GameficandoEstudos.Application.Services.MateriaService
{
    public class DeletarMateriaService
    {
        private readonly IMateriaRepository _materiaRepository;
        private readonly IAuditoriaService _auditoriaService;

        public DeletarMateriaService(IMateriaRepository materiaRepository, IAuditoriaService auditoriaService)
        {
            _materiaRepository = materiaRepository;
            _auditoriaService = auditoriaService;
        }

        public async Task ExecutarAsync(int id, int idProfessor)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(id);

            if (materia == null || materia.IdProfessor != idProfessor)
                throw new UnauthorizedAccessException("Matéria não encontrada ou você não tem permissão para excluí-la.");


            await _materiaRepository.DeletarAsync(id);
            await _auditoriaService.RegistrarAsync(idProfessor, "Materia.Deletar", "Materia", id.ToString());
        }
    }
}