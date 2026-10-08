using GameficandoEstudos.Application.Interfaces;


namespace GameficandoEstudos.Application.Services
{
    public class GeradorAtividadeService
    {
        private readonly IVectorSearchService _vectorSearch;
        private readonly ILlmService _llmService;

        public GeradorAtividadeService(IVectorSearchService vectorSearch, ILlmService llmService)
        {
            _vectorSearch = vectorSearch;
            _llmService = llmService;
        }


        public async Task<string> GerarAtividadeAsync(int idMateria, string temaDesejado)
        {
            var chunksRelevantes = await _vectorSearch.BuscarChunksRelevantesAsync(idMateria, temaDesejado);

            if (chunksRelevantes == null || !chunksRelevantes.Any())
            {
                throw new InvalidOperationException("Nenhuma fonte encontrada para essa matéria e tema");
            }

            var prompt = $"Crie uma atividade sobre o tema '{temaDesejado}' usando os seguintes conteúdos: {string.Join(", ", chunksRelevantes)}";

            var resposta = await _llmService.GerarRespostaAsync(prompt);

            return resposta;
        }

    }
}

