using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class GeradorAtividadeService
    {
        private readonly IVectorSearchService _vectorSearch;
        private readonly ILlmService _llmService;
        private readonly IAtividadeRepository _atividadeRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IAuditoriaService _auditoriaService;

        public GeradorAtividadeService(
            IVectorSearchService vectorSearch,
            ILlmService llmService,
            IAtividadeRepository atividadeRepository,
            IMateriaRepository materiaRepository,
            IAuditoriaService auditoriaService)
        {
            _vectorSearch = vectorSearch;
            _llmService = llmService;
            _atividadeRepository = atividadeRepository;
            _materiaRepository = materiaRepository;
            _auditoriaService = auditoriaService;
        }

        public async Task<string> GerarAtividadeAsync(int idMateria, string temaDesejado, TipoAtividade tipoAtividade, int idProfessorLogado)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                throw new InvalidOperationException("Matéria não encontrada");

            var chunksRelevantes = await _vectorSearch.BuscarChunksRelevantesAsync(idMateria, temaDesejado);

            if (chunksRelevantes == null || !chunksRelevantes.Any())
            {
                throw new InvalidOperationException("Nenhuma fonte encontrada para essa matéria e tema");
            }

            
            var prompt = $"Crie uma atividade do tipo '{tipoAtividade}' sobre o tema '{temaDesejado}' " +
                         $"usando os seguintes conteúdos: {string.Join(", ", chunksRelevantes)}. " +
                         "Formate a resposta EXCLUSIVAMENTE como um JSON válido com duas propriedades: " +
                         "\"conteudo\" (contendo o enunciado da questão e as opções de A a D, se for múltipla escolha) e " +
                         "\"gabarito\" (contendo apenas a resposta correta esperada, sem explicações). " +
                         "Não inclua formatação markdown como ```json, retorne apenas o objeto JSON puro.";

            var respostaLlm = await _llmService.GerarRespostaAsync(prompt);

            
            var jsonLimpo = respostaLlm.Replace("```json", "").Replace("```", "").Trim();

            
            RespostaIaDto atividadeGerada;
            try
            {
                atividadeGerada = JsonSerializer.Deserialize<RespostaIaDto>(jsonLimpo, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                                  ?? throw new InvalidOperationException("Falha ao desserializar o JSON do LLM.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"O LLM não retornou um JSON válido. Resposta original: {respostaLlm}", ex);
            }

            
            var novaAtividade = new Atividade(
                nome: temaDesejado,
                tipo: tipoAtividade,
                conteudo: atividadeGerada.Conteudo,
                gabarito: atividadeGerada.Gabarito,
                idMateria: idMateria
            );

            var atividadeCriada = await _atividadeRepository.AdicionarAsync(novaAtividade);

            await _auditoriaService.RegistrarAsync(
                idProfessorLogado, "Atividade.Gerar", "Atividade", atividadeCriada.Id.ToString(), $"Tema: {temaDesejado}");

            return jsonLimpo;
        }

        
        private record RespostaIaDto(string Conteudo, string Gabarito);
    }
}