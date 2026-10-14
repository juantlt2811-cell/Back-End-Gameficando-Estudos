
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.RankingService
{
    public class ObterRankingOfensivaService
    {
        private const int LimitePadrao = 20;

        private readonly IRankingRepository _rankingRepository;

        public ObterRankingOfensivaService(IRankingRepository rankingRepository)
        {
            _rankingRepository = rankingRepository;
        }

        public async Task<List<RankingOfensivaDto>> ExecutarAsync()
        {
            return await _rankingRepository.ObterRankingOfensivaAsync(LimitePadrao);
        }
    }
}