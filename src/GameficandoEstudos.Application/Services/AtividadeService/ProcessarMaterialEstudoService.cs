using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class ProcessarMaterialEstudoService
    {
        private const int TamanhoMaximoTextoCaracteres = 500_000;

        private readonly ILlmService _llmService;
        private readonly IMateriaRepository _materiaRepository;
        private readonly ICaixaFontesRepository _caixaFontesRepository;
        private readonly IAuditoriaService _auditoriaService;
        private readonly IDivisorDeTextoService _divisorDeTextoService;
        private readonly IModuloRepository _moduloRepository;

        public ProcessarMaterialEstudoService(
            ILlmService llmService,
            IMateriaRepository materiaRepository,
            ICaixaFontesRepository caixaFontesRepository,
            IAuditoriaService auditoriaService,
            IDivisorDeTextoService divisorDeTextoService,
            IModuloRepository moduloRepository)
        {
            _llmService = llmService;
            _materiaRepository = materiaRepository;
            _caixaFontesRepository = caixaFontesRepository;
            _auditoriaService = auditoriaService;
            _divisorDeTextoService = divisorDeTextoService;
            _moduloRepository = moduloRepository;
        }

        
        public async Task ProcessarTextoAsync(
            int idMateria, string tituloMaterial, string textoCompleto, int idProfessorLogado, int? idModulo = null)
        {
            if (textoCompleto.Length > TamanhoMaximoTextoCaracteres)
                throw new InvalidOperationException(
                    $"O texto do material excede o tamanho máximo permitido ({TamanhoMaximoTextoCaracteres.ToString("N0", new System.Globalization.CultureInfo("pt-BR"))} caracteres).");

            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                throw new InvalidOperationException("Matéria não encontrada.");

            if (materia.IdProfessor != idProfessorLogado)
                throw new UnauthorizedAccessException("Você não tem permissão para adicionar materiais nesta matéria.");

            if (idModulo.HasValue)
            {
                var modulo = await _moduloRepository.ObterPorIdAsync(idModulo.Value);
                if (modulo == null || modulo.IdMateria != idMateria)
                    throw new InvalidOperationException("Módulo não encontrado nesta matéria.");
            }

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

                novosChunks.Add(new FonteChunks(trecho, caixa.Id, embeddings, tituloMaterial, documentoId, idModulo));
            }

            if (novosChunks.Count > 0)
                await _caixaFontesRepository.AdicionarChunksAsync(novosChunks);

            await _auditoriaService.RegistrarAsync(
                idProfessorLogado, "CaixaFontes.AdicionarMaterial", "Materia", idMateria.ToString(),
                idModulo.HasValue ? $"Origem: {tituloMaterial} | Módulo: {idModulo}" : $"Origem: {tituloMaterial}");
        }
    }
}