using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace GameficandoEstudos.Infrastructure.Services;

public class VectorSearchService : IVectorSearchService
{
    private readonly AppDbContext _context;
    private readonly ILlmService _llmService;

    public VectorSearchService(AppDbContext context, ILlmService llmService)
    {
        _context = context;
        _llmService = llmService;
    }

    private const double LimiarDistanciaMaxima = 0.6;

    public async Task<IEnumerable<string>> BuscarChunksRelevantesAsync(int idMateria, string temaDesejado, int? idModulo)
    {
        var vetorBusca = new Vector(await _llmService.GerarEmbeddingsAsync(temaDesejado));

        var chunksRelevantes = await _context.FonteChunks
            .Where(fc => fc.CaixaFonte.IdMateria == idMateria)
            .Where(fc => idModulo == null || fc.IdModulo == idModulo)
            .Where(fc => fc.Embeddings.CosineDistance(vetorBusca) <= LimiarDistanciaMaxima)
            .OrderBy(fc => fc.Embeddings.CosineDistance(vetorBusca))
            .Take(5)
            .Select(fc => fc.Conteudo)
            .ToListAsync();

        return chunksRelevantes;
    }
}