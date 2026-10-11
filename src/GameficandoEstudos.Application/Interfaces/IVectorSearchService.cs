namespace GameficandoEstudos.Application.Interfaces
{
    public interface IVectorSearchService
    {
        Task<IEnumerable<string>> BuscarChunksRelevantesAsync(int idMateria, string temaDesejado);
    }
}
