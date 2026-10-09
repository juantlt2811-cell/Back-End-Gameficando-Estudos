using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface IAtividadeRepository
    {
        Task<Atividade> AdicionarAsync(Atividade atividade);
        Task<Atividade?> ObterPorIdAsync(int id);
        Task<IEnumerable<Atividade>> ObterPorMateriaAsync(int idMateria);
    }
}
