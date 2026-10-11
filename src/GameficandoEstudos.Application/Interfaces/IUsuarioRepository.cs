using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario> AdicionarAsync(Usuario usuario);
        Task<Usuario?> ObterPorIdAsync(int id);
        Task<IEnumerable<Usuario?>> ObterUsuariosPorTipoAsync(TipoUsuario tipoUsuario);
        Task<Usuario> AtualizarAsync(Usuario usuario);
        Task<bool> ExisteEmailAsync(string email);

        Task<Usuario?> ObterPorEmailAsync(string email);
    }
}
