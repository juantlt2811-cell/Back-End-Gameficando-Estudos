using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AdicionarAsync(RefreshToken refreshToken);
        Task<RefreshToken?> ObterPorHashAsync(string tokenHash);
        Task AtualizarAsync(RefreshToken refreshToken);
        Task RevogarTodosPorUsuarioAsync(int idUsuario);
    }
}