using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface IAtividadeRepository
    {
        Task<Atividade> AdicionarAsync(Atividade atividade);
        Task<Atividade?> ObterPorIdAsync(int id);
        Task<IEnumerable<Atividade>> ObterPorMateriaAsync(int idMateria);
        Task<bool> PossuiRespostasAsync(int idAtividade);
        Task RemoverAsync(Atividade atividade);
        Task AtualizarAsync(Atividade atividade);
        Task<List<Atividade>> ObterAleatoriasPorMateriaAsync(int idMateria, int quantidade);
        Task<int> ContarAtivasPorMateriaAsync(int idMateria);
    }
}
