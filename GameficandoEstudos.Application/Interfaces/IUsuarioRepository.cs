using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario> AdicionarAsync(Usuario usuario);
        Task<Usuario?> ObterPorIdAsync(int id);
        Task<IEnumerable<Usuario?>> ObterUsuariosPorTipoAsync(TipoUsuario tipoUsuario);
        Task<Usuario> AtualizarAsync(Usuario usuario);
    }
}
