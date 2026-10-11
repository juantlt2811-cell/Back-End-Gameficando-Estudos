using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class ProcessarMaterialEstudoService
    {
        private readonly ILlmService _llmService;
        private readonly IMateriaRepository _materiaRepository;
        private readonly ICaixaFontesRepository _caixaFontesRepository;

        public ProcessarMaterialEstudoService(ILlmService llmService, IMateriaRepository materiaRepository, ICaixaFontesRepository caixaFontesRepository)
        {
            _llmService = llmService;
            _materiaRepository = materiaRepository;
            _caixaFontesRepository = caixaFontesRepository;
        }

        public async Task ProcessarTextoAsync(int idMateria, string tituloMaterial, string textoCompleto, int idProfessorLogado)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                throw new InvalidOperationException("Matéria não encontrada.");

            if (materia.IdProfessor != idProfessorLogado)
                throw new UnauthorizedAccessException("Você não tem permissão para adicionar materiais nesta matéria.");

            var caixa = await _caixaFontesRepository.ObterPorMateriaAsync(idMateria);
            if (caixa == null)
            {
                caixa = new CaixaFontes($"Base de Conhecimento - Matéria {idMateria}", idMateria);
                caixa = await _caixaFontesRepository.AdicionarAsync(caixa);
            }
            Guid documentoId = Guid.NewGuid();

            var paragrafos = textoCompleto.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var paragrafo in paragrafos)
            {
                if (string.IsNullOrWhiteSpace(paragrafo) || paragrafo.Length < 10)
                    continue;

                var paragrafoAtual = await _llmService.GerarEmbeddingsAsync(paragrafo);

                var chunk = new FonteChunks(paragrafo, caixa.Id, paragrafoAtual, tituloMaterial, documentoId);

                caixa.AdicionarChunk(chunk);
            }

            await _caixaFontesRepository.AtualizarAsync(caixa);
        }
    }
}