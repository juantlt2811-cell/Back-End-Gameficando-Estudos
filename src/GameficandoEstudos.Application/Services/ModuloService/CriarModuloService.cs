using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Services.ModuloService
{
    public class CriarModuloService
    {
        private readonly IModuloRepository _moduloRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IAuditoriaService _auditoriaService;

        public CriarModuloService(
            IModuloRepository moduloRepository,
            IMateriaRepository materiaRepository,
            IAuditoriaService auditoriaService)
        {
            _moduloRepository = moduloRepository;
            _materiaRepository = materiaRepository;
            _auditoriaService = auditoriaService;
        }

        public async Task<ModuloDto> ExecutarAsync(CriarModuloDto dto, int idProfessorLogado)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(dto.IdMateria);
            if (materia == null || materia.IdProfessor != idProfessorLogado)
                throw new UnauthorizedAccessException("Matéria não encontrada ou você não tem permissão para alterá-la.");

            var modulo = new Modulo(dto.Nome, dto.IdMateria);

            if (await _moduloRepository.ExisteComNomeAsync(dto.IdMateria, modulo.Nome, null))
                throw new InvalidOperationException("Já existe um módulo com esse nome nesta matéria.");

            var criado = await _moduloRepository.AdicionarAsync(modulo);

            await _auditoriaService.RegistrarAsync(
                idProfessorLogado, "Modulo.Criar", "Modulo", criado.Id.ToString(), $"Matéria: {dto.IdMateria}");

            return new ModuloDto(criado.Id, criado.IdMateria, criado.Nome, 0, 0);
        }
    }
}
