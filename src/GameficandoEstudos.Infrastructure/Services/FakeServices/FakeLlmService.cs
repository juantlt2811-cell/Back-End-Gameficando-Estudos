using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Enums;
using System.Threading.Tasks;

namespace GameficandoEstudos.Infrastructure.Services.FakeServices
{
    public class FakeLlmService : ILlmService
    {
        public Task<string> GerarRespostaAsync(string prompt)
        {
            string jsonFake = "{\"Enunciado\": \"Pergunta gerada por IA (Modo Fake).\", " +
                               "\"Alternativas\": [\"A) Opção Correta\", \"B) Opção Errada\"], " +
                               "\"Gabarito\": \"A\"}";
            return Task.FromResult(jsonFake);
        }

        public Task<float[]> GerarEmbeddingsAsync(string texto)
        {
            return Task.FromResult(new float[] { 0.15f, 0.82f, 0.33f, 0.45f });
        }

        public Task<ResultadoAvaliacaoDto> AvaliarRespostaAsync(string conteudo, string respostaAluno, string gabarito, TipoAtividade tipo)
        {
            int pontuacao = respostaAluno.Length > 5 ? 100 : 30;
            string feedback = pontuacao >= ResultadoAvaliacaoDto.LimiarAprovacao
                ? "IA (Fake): A sua resposta está excelente e alinhada com o conteúdo!"
                : "IA (Fake): Resposta muito incompleta.";

            return Task.FromResult(new ResultadoAvaliacaoDto(pontuacao, feedback));
        }
    }
}