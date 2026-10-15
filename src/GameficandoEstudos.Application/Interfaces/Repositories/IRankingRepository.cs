using GameficandoEstudos.Application.DTOs;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface IRankingRepository
    {
        Task<List<RankingOfensivaDto>> ObterRankingOfensivaAsync(int limite);
        Task<List<RankingMateriaDto>> ObterRankingPorMateriaAsync(int idMateria, int limite, int minimoTentativas);
        Task<List<EstatisticaAlunoMateriaDto>> ObterEstatisticasPorMateriaAsync(int idMateria);
    }
}