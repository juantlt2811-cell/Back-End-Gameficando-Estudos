using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.DTOs;

namespace GameficandoEstudos.Application.Services.MateriaService
{
    public class AtualizarMateriaService
    {
        private readonly IMateriaRepository _materiaRepository;
        private readonly IAuditoriaService _auditoriaService;

        public AtualizarMateriaService(IMateriaRepository materiaRepository, IAuditoriaService auditoriaService)
        {
            _materiaRepository = materiaRepository;
            _auditoriaService = auditoriaService;
        }

        public async Task ExecutarAsync(AtualizarMateriaDto dto, int idProfessor)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(dto.Id);

            if (materia == null || materia.IdProfessor != idProfessor)
                throw new UnauthorizedAccessException("Matéria não encontrada ou você não tem permissão para editá-la.");

            materia.Atualizar(dto.Nome, dto.CargaHoraria, dto.Dificuldade);

            await _materiaRepository.AtualizarAsync(materia);
            await _auditoriaService.RegistrarAsync(idProfessor, "Materia.Atualizar", "Materia", dto.Id.ToString());
        }
    }
}