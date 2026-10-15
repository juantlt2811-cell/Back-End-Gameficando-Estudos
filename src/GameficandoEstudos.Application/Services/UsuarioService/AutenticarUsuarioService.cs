using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Interfaces.Security;
using GameficandoEstudos.Application.Validacoes;
using GameficandoEstudos.Domain.Entities;

public class AutenticarUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly ILoginAttemptGuard _loginAttemptGuard;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public AutenticarUsuarioService(
        IUsuarioRepository usuarioRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        ILoginAttemptGuard loginAttemptGuard,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _usuarioRepository = usuarioRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _loginAttemptGuard = loginAttemptGuard;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<AuthResultDto> ExecutarAsync(LoginDto dto)
    {
        if (_loginAttemptGuard.EstaBloqueado(dto.Email))
            throw new UnauthorizedAccessException("E-mail ou senha incorretos.");

        var usuario = await _usuarioRepository.ObterPorEmailAsync(dto.Email);
        if (usuario == null)
        {
            _loginAttemptGuard.RegistrarFalha(dto.Email);
            throw new UnauthorizedAccessException("E-mail ou senha incorretos.");
        }

        bool senhaValida = _passwordService.VerifyPassword(usuario.Senha, dto.Senha);
        if (!senhaValida)
        {
            _loginAttemptGuard.RegistrarFalha(dto.Email);
            throw new UnauthorizedAccessException("E-mail ou senha incorretos.");
        }

        _loginAttemptGuard.RegistrarSucesso(dto.Email);

        string accessToken = _tokenService.GerarToken(usuario);

        var refreshTokenBruto = TokenHashHelper.GerarTokenBruto();
        var refreshTokenHash = TokenHashHelper.GerarHash(refreshTokenBruto);
        int duracaoDias = _tokenService.ObterDuracaoRefreshTokenEmDias();

        var refreshToken = new RefreshToken(refreshTokenHash, usuario.Id, DateTime.UtcNow.AddDays(duracaoDias));
        await _refreshTokenRepository.AdicionarAsync(refreshToken);

        return new AuthResultDto(usuario.Id, usuario.Nome, accessToken, refreshTokenBruto, usuario.Tipo);
    }
}