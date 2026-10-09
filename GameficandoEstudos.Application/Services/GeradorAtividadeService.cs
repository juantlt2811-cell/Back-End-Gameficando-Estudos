using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;


namespace GameficandoEstudos.Application.Services
{
    public class GeradorAtividadeService
    {
        private readonly IVectorSearchService _vectorSearch;
        private readonly ILlmService _llmService;
        private readonly IAtividadeRepository _atividadeRepository;
        private readonly IMateriaRepository _materiaRepository;

        public GeradorAtividadeService(IVectorSearchService vectorSearch, ILlmService llmService, IAtividadeRepository atividadeRepository, IMateriaRepository materiaRepository)
        {
            _vectorSearch = vectorSearch;
            _llmService = llmService;
            _atividadeRepository = atividadeRepository;
            _materiaRepository = materiaRepository;
        }


        public async Task<string> GerarAtividadeAsync(int idMateria, string temaDesejado, string tipoAtividade)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                throw new InvalidOperationException("Matéria não encontrada");
            if(tipoAtividade == null)
                throw new ArgumentNullException(nameof(tipoAtividade), "Tipo de atividade não informado");

            var chunksRelevantes = await _vectorSearch.BuscarChunksRelevantesAsync(idMateria, temaDesejado);

            if (chunksRelevantes == null || !chunksRelevantes.Any())
            {
                throw new InvalidOperationException("Nenhuma fonte encontrada para essa matéria e tema");
            }

            var prompt = $"Crie uma atividade do tipo '{tipoAtividade}' sobre o tema '{temaDesejado}' " +
                $"usando os seguintes conteúdos: {string.Join(", ", chunksRelevantes)}. " +
                $"Formate adequadamente para esse tipo de atividade.";

            var resposta = await _llmService.GerarRespostaAsync(prompt);

            var novaAtividade = new Atividade(
                nome: temaDesejado,
                tipo: tipoAtividade,
                conteudo: resposta,
                idMateria: idMateria
            );
            await _atividadeRepository.AdicionarAsync(novaAtividade);
            return resposta;
        }

    }
}

