using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.UsuarioService
{
    public class AnonimizarContaService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IPasswordService _passwordService;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IAuditoriaService _auditoriaService;

        public AnonimizarContaService(
            IUsuarioRepository usuarioRepository,
            IPasswordService passwordService,
            IRefreshTokenRepository refreshTokenRepository,
            IAuditoriaService auditoriaService)
        {
            _usuarioRepository = usuarioRepository;
            _passwordService = passwordService;
            _refreshTokenRepository = refreshTokenRepository;
            _auditoriaService = auditoriaService;
        }

        public async Task ExecutarAsync(int idUsuario, string senhaAtual)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(idUsuario);
            if (usuario == null)
                throw new UnauthorizedAccessException("Usuário não encontrado.");

            if (!_passwordService.VerifyPassword(usuario.Senha, senhaAtual))
                throw new UnauthorizedAccessException("Senha incorreta.");

            var senhaInutilizavel = _passwordService.HashPassword(Guid.NewGuid().ToString("N"));
            usuario.Anonimizar(senhaInutilizavel);

            await _usuarioRepository.AtualizarAsync(usuario);
            await _refreshTokenRepository.RevogarTodosPorUsuarioAsync(idUsuario);

            await _auditoriaService.RegistrarAsync(idUsuario, "Usuario.Anonimizar", "Usuario", idUsuario.ToString());
        }
    }
}