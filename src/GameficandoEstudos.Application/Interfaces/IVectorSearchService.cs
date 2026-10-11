namespace GameficandoEstudos.Application.Interfaces
{
    public interface IVectorSearchService
    {
        Task<List<string>> BuscarChunksRelevantesAsync(int idMateria, string temaDesejado);
    }
}
