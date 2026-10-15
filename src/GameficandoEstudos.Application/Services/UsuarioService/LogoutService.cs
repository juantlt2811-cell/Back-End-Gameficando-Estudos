using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Validacoes;

namespace GameficandoEstudos.Application.Services.UsuarioService
{
    public class LogoutService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LogoutService(IRefreshTokenRepository refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task ExecutarAsync(string refreshTokenBruto)
        {
            var hash = TokenHashHelper.GerarHash(refreshTokenBruto);
            var refreshToken = await _refreshTokenRepository.ObterPorHashAsync(hash);

            if (refreshToken == null || refreshToken.RevogadoEm != null)
                return; 

            refreshToken.Revogar();
            await _refreshTokenRepository.AtualizarAsync(refreshToken);
        }
    }
}