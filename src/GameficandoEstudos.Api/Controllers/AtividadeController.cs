using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Services.AtividadeService;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AtividadeController : ControllerBase
    {
        private readonly GeradorAtividadeService _geradorAtividadeService;
        private readonly AtualizarAtividadeService _atualizarAtividadeService;
        private readonly ExcluirAtividadeService _excluirAtividadeService;
        private readonly ListarAtividadesDaMateriaService _listarAtividadesService;
        private readonly ResponderAtividadeService _responderAtividadeService;
        private readonly TestarAtividadeService _testarAtividadeService;

        public AtividadeController(
            GeradorAtividadeService geradorAtividadeService,
            AtualizarAtividadeService atualizarAtividadeService,
            ExcluirAtividadeService excluirAtividadeService,
            ListarAtividadesDaMateriaService listarAtividadesService,
            ResponderAtividadeService responderAtividadeService,
            TestarAtividadeService testarAtividadeService)
        {
            _geradorAtividadeService = geradorAtividadeService;
            _atualizarAtividadeService = atualizarAtividadeService;
            _excluirAtividadeService = excluirAtividadeService;
            _listarAtividadesService = listarAtividadesService;
            _responderAtividadeService = responderAtividadeService;
            _testarAtividadeService = testarAtividadeService;
        }

        [HttpGet("materia/{idMateria}")]
        public async Task<IActionResult> ListarPorMateria(int idMateria)
        {
            var tipoUsuario = ObterTipoUsuarioAutenticado();
            var atividades = await _listarAtividadesService.ExecutarAsync(idMateria, tipoUsuario);
            return Ok(atividades);
        }

        [HttpPost("gerar")]
        [Authorize(Roles = "Professor")] 
        public async Task<IActionResult> Gerar([FromBody] GerarAtividadeDto dto)
        {
            try
            {
                var tipo = (TipoAtividade)dto.TipoAtividade;
                var jsonLlm = await _geradorAtividadeService.GerarAtividadeAsync(dto.IdMateria, dto.Tema, tipo);
                return Ok(new { Mensagem = "Atividade gerada com sucesso", Detalhes = jsonLlm });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        [HttpPut("atualizar")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> Atualizar([FromBody] AtualizarAtividadeDto dto)
        {
            try
            {
                int idProfessor = ObterUsuarioIdAutenticado();
                await _atualizarAtividadeService.ExecutarAsync(dto, idProfessor);
                return Ok(new { Mensagem = "Atividade atualizada com sucesso!" });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                int idProfessor = ObterUsuarioIdAutenticado();
                await _excluirAtividadeService.ExecutarAsync(id, idProfessor);
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        [HttpPost("{id}/responder")]
        [Authorize(Roles = "Aluno")]
        public async Task<IActionResult> Responder(int id, [FromBody] EnviarRespostaDto dto)
        {
            try
            {
                int idAluno = ObterUsuarioIdAutenticado();
                var resultado = await _responderAtividadeService.ResponderAsync(idAluno, id, dto.Resposta);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        [HttpPost("{id}/testar")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> Testar(int id, [FromBody] EnviarRespostaDto dto)
        {
            try
            {
                int idProfessor = ObterUsuarioIdAutenticado();
                var resultado = await _testarAtividadeService.ExecutarAsync(idProfessor, id, dto.Resposta);
                return Ok(resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
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

        private TipoUsuario ObterTipoUsuarioAutenticado()
        {
            var claimRole = User.FindFirst(ClaimTypes.Role)?.Value;
            if (Enum.TryParse<TipoUsuario>(claimRole, out var tipo))
                return tipo;

            throw new UnauthorizedAccessException("Perfil de usuário não identificado no token.");
        }
    }
}