using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Validacoes;
using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Services.UsuarioService
{
    public class RefreshTokenService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ITokenService _tokenService;

        public RefreshTokenService(
            IRefreshTokenRepository refreshTokenRepository,
            IUsuarioRepository usuarioRepository,
            ITokenService tokenService)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _usuarioRepository = usuarioRepository;
            _tokenService = tokenService;
        }

        public async Task<AuthResultDto> ExecutarAsync(string refreshTokenBruto)
        {
            var hash = TokenHashHelper.GerarHash(refreshTokenBruto);
            var refreshTokenAtual = await _refreshTokenRepository.ObterPorHashAsync(hash);

            if (refreshTokenAtual == null || !refreshTokenAtual.EstaValido)
                throw new UnauthorizedAccessException("Sessão expirada. Faça login novamente.");

            var usuario = await _usuarioRepository.ObterPorIdAsync(refreshTokenAtual.IdUsuario);
            if (usuario == null)
                throw new UnauthorizedAccessException("Sessão expirada. Faça login novamente.");

            var novoTokenBruto = TokenHashHelper.GerarTokenBruto();
            var novoTokenHash = TokenHashHelper.GerarHash(novoTokenBruto);
            int duracaoDias = _tokenService.ObterDuracaoRefreshTokenEmDias();

            refreshTokenAtual.Revogar(novoTokenHash);
            await _refreshTokenRepository.AtualizarAsync(refreshTokenAtual);

            var novoRefreshToken = new RefreshToken(novoTokenHash, usuario.Id, DateTime.UtcNow.AddDays(duracaoDias));
            await _refreshTokenRepository.AdicionarAsync(novoRefreshToken);

            var novoAccessToken = _tokenService.GerarToken(usuario);

            return new AuthResultDto(usuario.Id, usuario.Nome, novoAccessToken, novoTokenBruto, usuario.Tipo);
        }
    }
}