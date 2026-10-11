using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.MateriaService
{
    public class ListarMateriasDisponiveisService
    {
        private readonly IMateriaRepository _materiaRepository;

        public ListarMateriasDisponiveisService(IMateriaRepository materiaRepository)
        {
            _materiaRepository = materiaRepository;
        }

        public async Task<IEnumerable<MateriaDto>> ExecutarAsync()
        {
            var materias = await _materiaRepository.ObterTodasAsync();

            return materias.Select(m => new MateriaDto(
                m.Id, m.Nome, m.CargaHoraria, m.Dificuldade, m.IdProfessor, m.CriadoEm
            ));
        }
    }
}