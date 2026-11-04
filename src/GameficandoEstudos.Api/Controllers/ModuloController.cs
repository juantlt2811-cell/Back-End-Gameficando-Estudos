using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Services.ModuloService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GameficandoEstudos.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ModuloController : ApiControllerBase
    {
        private readonly CriarModuloService _criarModuloService;
        private readonly RenomearModuloService _renomearModuloService;
        private readonly ExcluirModuloService _excluirModuloService;
        private readonly ListarModulosDaMateriaService _listarModulosService;

        public ModuloController(
            CriarModuloService criarModuloService,
            RenomearModuloService renomearModuloService,
            ExcluirModuloService excluirModuloService,
            ListarModulosDaMateriaService listarModulosService)
        {
            _criarModuloService = criarModuloService;
            _renomearModuloService = renomearModuloService;
            _excluirModuloService = excluirModuloService;
            _listarModulosService = listarModulosService;
        }

        [HttpGet("materia/{idMateria}")]
        public async Task<IActionResult> ListarPorMateria(int idMateria)
        {
            var modulos = await _listarModulosService.ExecutarAsync(idMateria);
            return Ok(modulos);
        }

        [HttpPost]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> Criar([FromBody] CriarModuloDto dto)
        {
            try
            {
                var modulo = await _criarModuloService.ExecutarAsync(dto, ObterUsuarioIdAutenticado());
                return Ok(modulo);
            }
            catch (UnauthorizedAccessException ex) { return Proibido(ex.Message); }
            catch (Exception ex) when (EhErroDeNegocio(ex)) { return BadRequest(new { Erro = ex.Message }); }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> Renomear(int id, [FromBody] RenomearModuloDto dto)
        {
            try
            {
                await _renomearModuloService.ExecutarAsync(id, dto, ObterUsuarioIdAutenticado());
                return Ok(new { Mensagem = "Módulo renomeado com sucesso!" });
            }
            catch (UnauthorizedAccessException ex) { return Proibido(ex.Message); }
            catch (Exception ex) when (EhErroDeNegocio(ex)) { return BadRequest(new { Erro = ex.Message }); }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Professor")]
        public async Task<IActionResult> Excluir(int id)
        {
            try
            {
                await _excluirModuloService.ExecutarAsync(id, ObterUsuarioIdAutenticado());
                return NoContent();
            }
            catch (UnauthorizedAccessException ex) { return Proibido(ex.Message); }
            catch (Exception ex) when (EhErroDeNegocio(ex)) { return BadRequest(new { Erro = ex.Message }); }
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
