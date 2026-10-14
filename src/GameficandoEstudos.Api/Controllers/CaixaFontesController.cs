using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Services.AtividadeService;
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
    [Authorize(Roles = "Professor")] 
    public class CaixaFontesController : ApiControllerBase
    {
        private readonly ProcessarMaterialEstudoService _processarMaterialService;
        private readonly RemoverMaterialEstudoService _removerMaterialService;
        private readonly ListarMateriaisDaMateriaService _listarMateriaisService;

        public CaixaFontesController(
            ProcessarMaterialEstudoService processarMaterialService,
            RemoverMaterialEstudoService removerMaterialService,
            ListarMateriaisDaMateriaService listarMateriaisService)
        {
            _processarMaterialService = processarMaterialService;
            _removerMaterialService = removerMaterialService;
            _listarMateriaisService = listarMateriaisService;
        }

        [HttpPost("processar-texto")]
        [EnableRateLimiting("ia")]
        public async Task<IActionResult> ProcessarTexto([FromBody] AdicionarMaterialDto dto)
        {
            try
            {
                int idProfessor = ObterUsuarioIdAutenticado();

                await _processarMaterialService.ProcessarTextoAsync(dto.IdMateria, dto.TituloMaterial, dto.TextoCompleto, idProfessor);

                return Ok(new { Mensagem = "Material processado e adicionado à base de conhecimento com sucesso." });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Proibido(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        [HttpDelete("remover-material")]
        public async Task<IActionResult> RemoverMaterial([FromBody] RemoverMaterialDto dto)
        {
            try
            {
                int idProfessor = ObterUsuarioIdAutenticado();

                await _removerMaterialService.ExecutarAsync(dto.IdMateria, dto.DocumentoId, idProfessor);

                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Proibido(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        [HttpGet("materia/{idMateria}/materiais")]
        public async Task<IActionResult> ListarMateriais(int idMateria, [FromQuery] string? termo)
        {
            try
            {
                int idProfessor = ObterUsuarioIdAutenticado();

                var request = new PesquisarMateriaisRequisicaoDto(idMateria, idProfessor, termo);
                var materiais = await _listarMateriaisService.ExecutarAsync(request);

                return Ok(materiais);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Proibido(ex.Message);
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
            {
                return usuarioId;
            }

            throw new UnauthorizedAccessException("Usuário não autenticado ou token JWT inválido.");
        }
    }
}