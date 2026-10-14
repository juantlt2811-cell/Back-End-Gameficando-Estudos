using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Services.SessaoPraticaService;
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
    [Authorize(Roles = "Aluno")]
    public class SessaoPraticaController : ApiControllerBase
    {
        private readonly IniciarSessaoPraticaService _iniciarService;
        private readonly ResponderSessaoPraticaService _responderService;
        private readonly ConcluirSessaoPraticaService _concluirService;

        public SessaoPraticaController(
            IniciarSessaoPraticaService iniciarService,
            ResponderSessaoPraticaService responderService,
            ConcluirSessaoPraticaService concluirService)
        {
            _iniciarService = iniciarService;
            _responderService = responderService;
            _concluirService = concluirService;
        }

        [HttpPost("iniciar/{idMateria}")]
        public async Task<IActionResult> Iniciar(int idMateria)
        {
            try
            {
                var resultado = await _iniciarService.ExecutarAsync(ObterUsuarioIdAutenticado(), idMateria);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex) { return Proibido(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(new { Erro = ex.Message }); }
        }

        [HttpPost("{idSessao}/responder/{idAtividade}")]
        [EnableRateLimiting("ia")]
        public async Task<IActionResult> Responder(int idSessao, int idAtividade, [FromBody] EnviarRespostaDto dto)
        {
            try
            {
                var resultado = await _responderService.ExecutarAsync(ObterUsuarioIdAutenticado(), idSessao, idAtividade, dto.Resposta);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex) { return Proibido(ex.Message); }
            catch (Exception ex) { return BadRequest(new { Erro = ex.Message }); }
        }

        [HttpPost("{idSessao}/concluir")]
        public async Task<IActionResult> Concluir(int idSessao)
        {
            try
            {
                var resumo = await _concluirService.ExecutarAsync(ObterUsuarioIdAutenticado(), idSessao);
                return Ok(resumo);
            }
            catch (UnauthorizedAccessException ex) { return Proibido(ex.Message); }
            catch (InvalidOperationException ex) { return BadRequest(new { Erro = ex.Message }); }
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