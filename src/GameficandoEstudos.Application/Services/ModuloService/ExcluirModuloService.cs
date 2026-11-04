using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.ModuloService
{
    public class ExcluirModuloService
    {
        private readonly IModuloRepository _moduloRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IAuditoriaService _auditoriaService;

        public ExcluirModuloService(
            IModuloRepository moduloRepository,
            IMateriaRepository materiaRepository,
            IAuditoriaService auditoriaService)
        {
            _moduloRepository = moduloRepository;
            _materiaRepository = materiaRepository;
            _auditoriaService = auditoriaService;
        }

        
        public async Task ExecutarAsync(int idModulo, int idProfessorLogado)
        {
            var modulo = await _moduloRepository.ObterPorIdAsync(idModulo);
            if (modulo == null)
                throw new InvalidOperationException("Módulo não encontrado.");

            var materia = await _materiaRepository.ObterPorIdAsync(modulo.IdMateria);
            if (materia == null || materia.IdProfessor != idProfessorLogado)
                throw new UnauthorizedAccessException("Você não tem permissão para excluir módulos desta matéria.");

            await _moduloRepository.RemoverAsync(modulo);

            await _auditoriaService.RegistrarAsync(
                idProfessorLogado, "Modulo.Excluir", "Modulo", idModulo.ToString(), $"Nome: {modulo.Nome}");
        }
    }
}
