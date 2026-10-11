using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.MateriaService
{
    public class ListarMinhasMateriasService
    {
        private readonly IMateriaRepository _materiaRepository;

        public ListarMinhasMateriasService(IMateriaRepository materiaRepository)
        {
            _materiaRepository = materiaRepository;
        }

        public async Task<IEnumerable<MateriaDto>> ExecutarAsync(int idProfessor)
        {
            var materias = await _materiaRepository.ObterPorProfessorAsync(idProfessor);

            return materias.Select(m => new MateriaDto(
                m.Id, m.Nome, m.CargaHoraria, m.Dificuldade, m.IdProfessor, m.CriadoEm
            ));
        }
    }
}