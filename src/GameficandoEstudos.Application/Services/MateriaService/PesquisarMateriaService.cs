using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.MateriaService
{
    public class PesquisarMateriaService
    {
        private readonly IMateriaRepository _materiaRepository;

        public PesquisarMateriaService(IMateriaRepository materiaRepository)
        {
            _materiaRepository = materiaRepository;
        }

        public async Task<IEnumerable<MateriaDto>> ExecutarAsync(string? termoPesquisa, int? idProfessor)
        {
            if (string.IsNullOrWhiteSpace(termoPesquisa) && !idProfessor.HasValue)
                return Enumerable.Empty<MateriaDto>();

            var materias = await _materiaRepository.PesquisarAsync(termoPesquisa, idProfessor);

            return materias.Select(m => new MateriaDto(
                m.Id, m.Nome, m.CargaHoraria, m.Dificuldade, m.IdProfessor, m.CriadoEm
            ));
        }
    }
}