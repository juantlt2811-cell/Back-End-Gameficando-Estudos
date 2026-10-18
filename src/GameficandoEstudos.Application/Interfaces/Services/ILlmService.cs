using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Application.Interfaces
{
    public interface ILlmService
    {
        Task<string> GerarRespostaAsync(string prompt);
        Task<float[]> GerarEmbeddingsAsync(string texto);
        Task<ResultadoAvaliacaoDto> AvaliarRespostaAsync(string enunciado, string respostaAluno, string gabaritoEsperado, TipoAtividade tipo);
    }
}