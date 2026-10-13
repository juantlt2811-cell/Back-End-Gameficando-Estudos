using GameficandoEstudos.Application.Services;
using GameficandoEstudos.Application.Services.MateriaService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using GameficandoEstudos.Application.DTOs;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GameficandoEstudos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] 
    public class MateriaController : ApiControllerBase
    {
        private readonly CriarMateriaService _criarMateriaService;
        private readonly AtualizarMateriaService _atualizarMateriaService;
        private readonly DeletarMateriaService _deletarMateriaService;
        private readonly BuscarMateriaPorIdService _buscarMateriaPorIdService;
        private readonly ListarMinhasMateriasService _listarMinhasMateriasService;
        private readonly ListarMateriasDisponiveisService _listarMateriasDisponiveisService;
        private readonly PesquisarMateriaService _pesquisarMateriaService;

        public MateriaController(
            CriarMateriaService criarMateriaService,
            AtualizarMateriaService atualizarMateriaService,
            DeletarMateriaService deletarMateriaService,
            BuscarMateriaPorIdService buscarMateriaPorIdService,
            ListarMinhasMateriasService listarMinhasMateriasService,
            ListarMateriasDisponiveisService listarMateriasDisponiveisService,
            PesquisarMateriaService pesquisarMateriaService)
        {
            _criarMateriaService = criarMateriaService;
            _atualizarMateriaService = atualizarMateriaService;
            _deletarMateriaService = deletarMateriaService;
            _buscarMateriaPorIdService = buscarMateriaPorIdService;
            _listarMinhasMateriasService = listarMinhasMateriasService;
            _listarMateriasDisponiveisService = listarMateriasDisponiveisService;
            _pesquisarMateriaService = pesquisarMateriaService;
        }

        [HttpPost("criar")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> Criar([FromBody] CriarMateriaDto dto)
        {
            try
            {
                int idProfessor = ObterUsuarioIdAutenticado();
                var idMateria = await _criarMateriaService.ExecutarAsync(dto, idProfessor);

                return CreatedAtAction(nameof(BuscarPorId), new { id = idMateria }, new { Mensagem = "Matéria criada com sucesso!", Id = idMateria });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Erro = ex.Message });
            }
        }

        [HttpPut("atualizar")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> Atualizar([FromBody] AtualizarMateriaDto dto)
        {
            try
            {
                int idProfessor = ObterUsuarioIdAutenticado();
                await _atualizarMateriaService.ExecutarAsync(dto, idProfessor);
                return Ok(new { Mensagem = "Matéria atualizada com sucesso!" });
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

        [HttpDelete("{id}")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> Deletar(int id)
        {
            try
            {
                int idProfessor = ObterUsuarioIdAutenticado();
                await _deletarMateriaService.ExecutarAsync(id, idProfessor);
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

        [HttpGet("{id}")]
        public async Task<IActionResult> BuscarPorId(int id)
        {
            try
            {
                int idUsuario = ObterUsuarioIdAutenticado();
                var materia = await _buscarMateriaPorIdService.ExecutarAsync(id, idUsuario);
                return Ok(materia);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Proibido(ex.Message);
            }
            catch (Exception ex)
            {
                return NotFound(new { Erro = ex.Message });
            }
        }

        [HttpGet("minhas")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> ListarMinhas()
        {
            int idProfessor = ObterUsuarioIdAutenticado();
            var materias = await _listarMinhasMateriasService.ExecutarAsync(idProfessor);
            return Ok(materias);
        }

        [HttpGet("disponiveis")]
        public async Task<IActionResult> ListarDisponiveis()
        {
            var materias = await _listarMateriasDisponiveisService.ExecutarAsync();
            return Ok(materias);
        }

        [HttpGet("pesquisar")]
        public async Task<IActionResult> Pesquisar([FromQuery] string? termo, [FromQuery] int? idProfessor)
        {
            var materias = await _pesquisarMateriaService.ExecutarAsync(termo, idProfessor);
            return Ok(materias);
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