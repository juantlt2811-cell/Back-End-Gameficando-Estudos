using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class RemoverMaterialEstudoService
    {
        private readonly IMateriaRepository _materiaRepository;
        private readonly ICaixaFontesRepository _caixaFontesRepository;

        public RemoverMaterialEstudoService(IMateriaRepository materiaRepository, ICaixaFontesRepository caixaFontesRepository)
        {
            _materiaRepository = materiaRepository;
            _caixaFontesRepository = caixaFontesRepository;
        }

        public async Task ExecutarAsync(int idMateria, Guid documentoId, int idProfessorLogado)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                throw new InvalidOperationException("Matéria não encontrada.");

            if (materia.IdProfessor != idProfessorLogado)
                throw new UnauthorizedAccessException("Você não tem permissão para remover materiais desta matéria.");

            var caixa = await _caixaFontesRepository.ObterPorMateriaAsync(idMateria);
            if (caixa == null)
                throw new InvalidOperationException("Esta matéria ainda não possui uma base de conhecimento.");

            await _caixaFontesRepository.RemoverChunksPorDocumentoIdAsync(documentoId);
        }
    }
}