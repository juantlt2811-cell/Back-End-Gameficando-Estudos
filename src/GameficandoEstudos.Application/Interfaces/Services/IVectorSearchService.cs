namespace GameficandoEstudos.Application.Interfaces
{
    public interface IVectorSearchService
    {
        /// <summary>
        /// Busca os trechos de material mais relevantes para o tema. Com <paramref name="idModulo"/>,
        /// procura só nos materiais daquele módulo; com <c>null</c>, em todos os materiais da matéria.
        /// </summary>
        Task<IEnumerable<string>> BuscarChunksRelevantesAsync(int idMateria, string temaDesejado, int? idModulo);
    }
}
