using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class MoverMaterialDeModuloService
    {
        private readonly IMateriaRepository _materiaRepository;
        private readonly IModuloRepository _moduloRepository;
        private readonly ICaixaFontesRepository _caixaFontesRepository;
        private readonly IAuditoriaService _auditoriaService;

        public MoverMaterialDeModuloService(
            IMateriaRepository materiaRepository,
            IModuloRepository moduloRepository,
            ICaixaFontesRepository caixaFontesRepository,
            IAuditoriaService auditoriaService)
        {
            _materiaRepository = materiaRepository;
            _moduloRepository = moduloRepository;
            _caixaFontesRepository = caixaFontesRepository;
            _auditoriaService = auditoriaService;
        }

        
        public async Task ExecutarAsync(int idMateria, Guid documentoId, int? idModulo, int idProfessorLogado)
        {
            if (documentoId == Guid.Empty)
                throw new ArgumentException("Id do material inválido.", nameof(documentoId));

            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                throw new InvalidOperationException("Matéria não encontrada.");

            if (materia.IdProfessor != idProfessorLogado)
                throw new UnauthorizedAccessException("Você não tem permissão para alterar materiais desta matéria.");

            if (idModulo.HasValue)
            {
                var modulo = await _moduloRepository.ObterPorIdAsync(idModulo.Value);
                if (modulo == null || modulo.IdMateria != idMateria)
                    throw new InvalidOperationException("Módulo não encontrado nesta matéria.");
            }

            var trechosMovidos = await _caixaFontesRepository.MoverDocumentoDeModuloAsync(idMateria, documentoId, idModulo);
            if (trechosMovidos == 0)
                throw new InvalidOperationException("Material não encontrado nesta matéria.");

            await _auditoriaService.RegistrarAsync(
                idProfessorLogado, "CaixaFontes.MoverMaterial", "Materia", idMateria.ToString(),
                idModulo.HasValue ? $"DocumentoId: {documentoId} | Módulo: {idModulo}" : $"DocumentoId: {documentoId} | Sem módulo");
        }
    }
}
