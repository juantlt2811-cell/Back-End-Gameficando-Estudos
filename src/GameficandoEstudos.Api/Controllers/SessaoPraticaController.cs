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
        private readonly VerificarPodeIniciarSessaoService _verificarPodeIniciarService;


        public SessaoPraticaController(
            IniciarSessaoPraticaService iniciarService,
            ResponderSessaoPraticaService responderService,
            ConcluirSessaoPraticaService concluirService,
            VerificarPodeIniciarSessaoService verificarPodeIniciarService)
        {
            _iniciarService = iniciarService;
            _responderService = responderService;
            _concluirService = concluirService;
            _verificarPodeIniciarService = verificarPodeIniciarService;
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
            catch (Exception ex) when (EhErroDeNegocio(ex)) { return BadRequest(new { Erro = ex.Message }); }
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
        [HttpGet("confirmar-inicio/{idMateria}")]
        public async Task<IActionResult> ConfirmarInicio(int idMateria)
        {
            try
            {
                var confirmacao = await _verificarPodeIniciarService.ExecutarAsync(ObterUsuarioIdAutenticado(), idMateria);
                return Ok(confirmacao);
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