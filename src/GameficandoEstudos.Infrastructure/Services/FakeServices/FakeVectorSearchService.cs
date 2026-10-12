using GameficandoEstudos.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameficandoEstudos.Infrastructure.Services.FakeServices
{
    public class FakeVectorSearchService : IVectorSearchService
    {
        public Task<IEnumerable<string>> BuscarChunksRelevantesAsync(int idMateria, string temaDesejado)
        {
            var chunksFakes = new List<string>
            {
                $"Trecho relevante encontrado na base sobre o tema '{temaDesejado}'.",
                "Outro conceito importante extraído dos PDFs da matéria para ajudar a formular a questão."
            };

            return Task.FromResult<IEnumerable<string>>(chunksFakes);
        }
    }
}