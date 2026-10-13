using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Services.UsuarioService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.Threading.Tasks;

namespace GameficandoEstudos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ApiControllerBase
    {
        private readonly CadastrarUsuarioService _cadastrarService;
        private readonly AutenticarUsuarioService _autenticarService;

        public AuthController(CadastrarUsuarioService cadastrarService, AutenticarUsuarioService autenticarService)
        {
            _cadastrarService = cadastrarService;
            _autenticarService = autenticarService;
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
    }
}