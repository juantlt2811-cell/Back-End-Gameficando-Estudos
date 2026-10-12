using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace GameficandoEstudos.Infrastructure.Services;

public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly string _groqApiKey;
    private readonly string _embeddingsApiKey;

    public LlmService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;

        _groqApiKey = configuration["LlmApi:GroqKey"] ?? throw new ArgumentNullException("GroqKey não configurada");
        _embeddingsApiKey = configuration["LlmApi:EmbeddingsKey"] ?? throw new ArgumentNullException("EmbeddingsKey não configurada");
    }

    public async Task<string> GerarRespostaAsync(string prompt)
    {
        var requestBody = new
        {
            model = "llama3-8b-8192", 
            messages = new[] { new { role = "user", content = prompt } },
            temperature = 0.7
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseString);

        return jsonDocument.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString() ?? string.Empty;
    }

    public async Task<float[]> GerarEmbeddingsAsync(string texto)
    {
        var requestBody = new
        {
            model = "models/text-embedding-004",
            content = new
            {
                parts = new[] { new { text = texto } }
            }
        };

        // O Gemini recebe a chave diretamente via Query String na URL
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/text-embedding-004:embedContent?key={_embeddingsApiKey}";

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseString);

        // O Gemini devolve o array de floats dentro de embedding -> values
        return jsonDocument.RootElement
            .GetProperty("embedding")
            .GetProperty("values")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();
    }

    public async Task<ResultadoAvaliacaoDto> AvaliarRespostaAsync(string enunciado, string respostaAluno, string gabaritoEsperado)
    {
        var prompt = $@"
            Atue como um professor rigoroso. Avalie a resposta do aluno com base no gabarito.
    
            Enunciado: {enunciado}
            Gabarito Esperado: {gabaritoEsperado}
            Resposta do Aluno: {respostaAluno}

            Devolva a resposta EXCLUSIVAMENTE em formato JSON com as seguintes propriedades exatas:
            - ""aprovado"": boolean (true se a ideia central estiver certa e demonstrar entendimento, false se estiver errada).
            - ""feedback"": texto curto e direto justificando a avaliação.
        ";

        var requestBody = new
        {
            model = "llama3-8b-8192",
            messages = new[] { new { role = "user", content = prompt } },
            temperature = 0.2, 
            response_format = new { type = "json_object" } 
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqApiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        using var jsonDocument = JsonDocument.Parse(responseString);

        var jsonContent = jsonDocument.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        var resultado = JsonSerializer.Deserialize<ResultadoAvaliacaoDto>(jsonContent ?? "{}", new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return resultado ?? new ResultadoAvaliacaoDto(false, "Ocorreu um erro ao contactar a IA para avaliação. Por favor, tente novamente.");
    }
}