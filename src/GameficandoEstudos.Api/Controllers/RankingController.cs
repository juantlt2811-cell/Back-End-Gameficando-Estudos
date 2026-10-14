using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GameficandoEstudos.Application.Services.RankingService;

namespace GameficandoEstudos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RankingController : ApiControllerBase
    {
        private readonly ObterRankingOfensivaService _rankingOfensivaService;
        private readonly ObterRankingMateriaService _rankingMateriaService;

        public RankingController(
            ObterRankingOfensivaService rankingOfensivaService,
            ObterRankingMateriaService rankingMateriaService)
        {
            _rankingOfensivaService = rankingOfensivaService;
            _rankingMateriaService = rankingMateriaService;
        }

        [HttpGet("ofensiva")]
        public async Task<IActionResult> RankingOfensiva()
        {
            var ranking = await _rankingOfensivaService.ExecutarAsync();
            return Ok(ranking);
        }

        [HttpGet("materia/{idMateria}")]
        public async Task<IActionResult> RankingMateria(int idMateria)
        {
            try
            {
                var ranking = await _rankingMateriaService.ExecutarAsync(idMateria);
                return Ok(ranking);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }
    }
}