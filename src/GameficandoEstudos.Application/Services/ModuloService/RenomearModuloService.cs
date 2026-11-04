using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.ModuloService
{
    public class RenomearModuloService
    {
        private readonly IModuloRepository _moduloRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IAuditoriaService _auditoriaService;

        public RenomearModuloService(
            IModuloRepository moduloRepository,
            IMateriaRepository materiaRepository,
            IAuditoriaService auditoriaService)
        {
            _moduloRepository = moduloRepository;
            _materiaRepository = materiaRepository;
            _auditoriaService = auditoriaService;
        }

        public async Task ExecutarAsync(int idModulo, RenomearModuloDto dto, int idProfessorLogado)
        {
            var modulo = await _moduloRepository.ObterPorIdAsync(idModulo);
            if (modulo == null)
                throw new InvalidOperationException("Módulo não encontrado.");

            var materia = await _materiaRepository.ObterPorIdAsync(modulo.IdMateria);
            if (materia == null || materia.IdProfessor != idProfessorLogado)
                throw new UnauthorizedAccessException("Você não tem permissão para alterar módulos desta matéria.");

            modulo.Renomear(dto.Nome);

            if (await _moduloRepository.ExisteComNomeAsync(modulo.IdMateria, modulo.Nome, modulo.Id))
                throw new InvalidOperationException("Já existe um módulo com esse nome nesta matéria.");

            await _moduloRepository.AtualizarAsync(modulo);

            await _auditoriaService.RegistrarAsync(
                idProfessorLogado, "Modulo.Renomear", "Modulo", modulo.Id.ToString());
        }
    }
}
