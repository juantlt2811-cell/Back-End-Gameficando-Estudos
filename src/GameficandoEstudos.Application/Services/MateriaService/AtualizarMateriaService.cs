using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.MateriaService
{
    public class AtualizarMateriaService
    {
        private readonly IMateriaRepository _materiaRepository;

        public AtualizarMateriaService(IMateriaRepository materiaRepository)
        {
            _materiaRepository = materiaRepository;
        }

        public async Task ExecutarAsync(AtualizarMateriaDto dto, int idProfessor)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(dto.Id);

            if (materia == null || materia.IdProfessor != idProfessor)
                throw new UnauthorizedAccessException("Matéria não encontrada ou você não tem permissão para editá-la.");

            materia.Atualizar(dto.Nome, dto.CargaHoraria, dto.Dificuldade);

            await _materiaRepository.AtualizarAsync(materia);
        }
    }
}