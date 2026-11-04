using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface IModuloRepository
    {
        Task<Modulo> AdicionarAsync(Modulo modulo);
        Task<Modulo?> ObterPorIdAsync(int id);
        Task<List<ModuloDto>> ListarPorMateriaAsync(int idMateria);

        Task<bool> ExisteComNomeAsync(int idMateria, string nome, int? ignorarIdModulo);

        Task AtualizarAsync(Modulo modulo);
        Task RemoverAsync(Modulo modulo);
    }
}
