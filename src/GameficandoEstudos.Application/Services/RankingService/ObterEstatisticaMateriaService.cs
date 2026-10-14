
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.RankingService
{
    public class ObterEstatisticasMateriaService
    {
        private readonly IRankingRepository _rankingRepository;
        private readonly IMateriaRepository _materiaRepository;

        public ObterEstatisticasMateriaService(IRankingRepository rankingRepository, IMateriaRepository materiaRepository)
        {
            _rankingRepository = rankingRepository;
            _materiaRepository = materiaRepository;
        }

        public async Task<List<EstatisticaAlunoMateriaDto>> ExecutarAsync(int idMateria, int idProfessorLogado)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                throw new InvalidOperationException("Matéria não encontrada.");

            if (materia.IdProfessor != idProfessorLogado)
                throw new UnauthorizedAccessException("Você não tem permissão para ver estatísticas de matérias de outros professores.");

            return await _rankingRepository.ObterEstatisticasPorMateriaAsync(idMateria);
        }
    }
}