
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.RankingService
{
    public class ObterRankingMateriaService
    {
        private const int LimitePadrao = 20;
        private const int MinimoTentativas = 3;

        private readonly IRankingRepository _rankingRepository;
        private readonly IMateriaRepository _materiaRepository;

        public ObterRankingMateriaService(IRankingRepository rankingRepository, IMateriaRepository materiaRepository)
        {
            _rankingRepository = rankingRepository;
            _materiaRepository = materiaRepository;
        }

        public async Task<List<RankingMateriaDto>> ExecutarAsync(int idMateria)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                throw new InvalidOperationException("Matéria não encontrada.");

            return await _rankingRepository.ObterRankingPorMateriaAsync(idMateria, LimitePadrao, MinimoTentativas);
        }
    }
}