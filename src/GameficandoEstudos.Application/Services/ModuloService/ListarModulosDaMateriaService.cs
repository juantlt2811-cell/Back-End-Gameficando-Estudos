using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.ModuloService
{
    public class ListarModulosDaMateriaService
    {
        private readonly IModuloRepository _moduloRepository;
        private readonly IMateriaRepository _materiaRepository;

        public ListarModulosDaMateriaService(IModuloRepository moduloRepository, IMateriaRepository materiaRepository)
        {
            _moduloRepository = moduloRepository;
            _materiaRepository = materiaRepository;
        }

        
        public async Task<IEnumerable<ModuloDto>> ExecutarAsync(int idMateria)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                return Enumerable.Empty<ModuloDto>();

            return await _moduloRepository.ListarPorMateriaAsync(idMateria);
        }
    }
}
