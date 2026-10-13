using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Services.UsuarioService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GameficandoEstudos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ApiControllerBase
    {
        private readonly CadastrarUsuarioService _cadastrarService;
        private readonly AutenticarUsuarioService _autenticarService;
        private readonly SolicitarRedefinicaoSenhaService _solicitarRedefinicaoSenhaService;
        private readonly RedefinirSenhaService _redefinirSenhaService;
        private readonly AlterarSenhaService _alterarSenhaService;
        public AuthController(CadastrarUsuarioService cadastrarService, 
            AutenticarUsuarioService autenticarService, 
            SolicitarRedefinicaoSenhaService solicitarRedefinicaoSenhaService, 
            RedefinirSenhaService redifinirSenhaService,
            AlterarSenhaService alterarSenhaService)
        {
            _cadastrarService = cadastrarService;
            _autenticarService = autenticarService;
            _solicitarRedefinicaoSenhaService = solicitarRedefinicaoSenhaService;
            _redefinirSenhaService = redifinirSenhaService;
            _alterarSenhaService = alterarSenhaService;
        }

        [HttpPost("cadastrar")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Cadastrar([FromBody] RegistrarUsuarioDto dto)
        {
            try
            {
                var id = await _cadastrarService.ExecutarAsync(dto);
                return Created("", new { Id = id, Mensagem = "Usuário cadastrado com sucesso!" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        [HttpPost("login")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var resultado = await _autenticarService.ExecutarAsync(dto);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Erro = ex.Message });
            }
        }
        [HttpPost("esqueci-senha")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> EsqueciSenha([FromBody] SolicitarRedefinicaoSenhaDto dto)
        {
            await _solicitarRedefinicaoSenhaService.ExecutarAsync(dto);
            return Ok(new { Mensagem = "Se o e-mail informado existir em nossa base, você receberá instruções para redefinir sua senha." });
        }

        [HttpPost("redefinir-senha")]
        [EnableRateLimiting("auth")]
        public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaDto dto)
        {
            try
            {
                await _redefinirSenhaService.ExecutarAsync(dto);
                return Ok(new { Mensagem = "Senha redefinida com sucesso." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }
        [HttpPut("alterar-senha")]
        [Authorize]
        public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
        {
            int idUsuario = ObterUsuarioIdAutenticado(); 

            try
            {
                await _alterarSenhaService.ExecutarAsync(idUsuario, dto);
                return Ok(new { Mensagem = "Senha alterada com sucesso." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Proibido(ex.Message); 
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }
        private int ObterUsuarioIdAutenticado()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null && int.TryParse(claim.Value, out int usuarioId))
                return usuarioId;

            throw new UnauthorizedAccessException("Usuário não autenticado ou token JWT inválido.");
        }
    }
}