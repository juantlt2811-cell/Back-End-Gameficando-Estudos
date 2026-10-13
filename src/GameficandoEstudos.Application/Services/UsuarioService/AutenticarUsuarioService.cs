using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.Interfaces.Security;

public class AutenticarUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly ILoginAttemptGuard _loginAttemptGuard;

    public AutenticarUsuarioService(
        IUsuarioRepository usuarioRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        ILoginAttemptGuard loginAttemptGuard)
    {
        _usuarioRepository = usuarioRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _loginAttemptGuard = loginAttemptGuard;
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

        string token = _tokenService.GerarToken(usuario);
        return new AuthResultDto(usuario.Id, usuario.Nome, token, usuario.Tipo);
    }
}