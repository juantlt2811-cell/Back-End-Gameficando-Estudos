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
        private readonly IAuditoriaService _auditoriaService;
        private readonly IDivisorDeTextoService _divisorDeTextoService;

        public ProcessarMaterialEstudoService(
            ILlmService llmService, 
            IMateriaRepository materiaRepository, 
            ICaixaFontesRepository caixaFontesRepository, 
            IAuditoriaService auditoriaService, 
            IDivisorDeTextoService divisorDeTextoService)
        {
            _llmService = llmService;
            _materiaRepository = materiaRepository;
            _caixaFontesRepository = caixaFontesRepository;
            _auditoriaService = auditoriaService;
            _divisorDeTextoService = divisorDeTextoService;
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

            var trechos = _divisorDeTextoService.Dividir(textoCompleto);

            var novosChunks = new List<FonteChunks>();

            foreach (var trecho in trechos)
            {
                var embeddings = await _llmService.GerarEmbeddingsAsync(trecho);

                novosChunks.Add(new FonteChunks(trecho, caixa.Id, embeddings, tituloMaterial, documentoId));
            }

            if (novosChunks.Count > 0)
                await _caixaFontesRepository.AdicionarChunksAsync(novosChunks);

            await _auditoriaService.RegistrarAsync(
                idProfessorLogado, "CaixaFontes.AdicionarMaterial", "Materia", idMateria.ToString(), $"Origem: {tituloMaterial}");
        }
    }
}