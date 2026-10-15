using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GameficandoEstudos.Application.Services.UsuarioService;
using GameficandoEstudos.Application.DTOs;

namespace GameficandoEstudos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuarioController : ApiControllerBase
    {
        private readonly ExportarDadosUsuarioService _exportarDadosService;
        private readonly AnonimizarContaService _anonimizarContaService;

        public UsuarioController(
            ExportarDadosUsuarioService exportarDadosService,
            AnonimizarContaService anonimizarContaService)
        {
            _exportarDadosService = exportarDadosService;
            _anonimizarContaService = anonimizarContaService;
        }

        [HttpGet("meus-dados")]
        public async Task<IActionResult> MeusDados()
        {
            try
            {
                var dados = await _exportarDadosService.ExecutarAsync(ObterUsuarioIdAutenticado());
                return Ok(dados);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        [HttpDelete("minha-conta")]
        public async Task<IActionResult> MinhaConta([FromBody] AnonimizarContaDto dto)
        {
            try
            {
                await _anonimizarContaService.ExecutarAsync(ObterUsuarioIdAutenticado(), dto.SenhaAtual);
                return Ok(new { Mensagem = "Conta anonimizada com sucesso. Seus dados pessoais foram removidos." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Proibido(ex.Message);
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