using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class GeradorAtividadeService
    {
        private readonly IVectorSearchService _vectorSearch;
        private readonly ILlmService _llmService;
        private readonly IAtividadeRepository _atividadeRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IAuditoriaService _auditoriaService;

        public GeradorAtividadeService(
            IVectorSearchService vectorSearch,
            ILlmService llmService,
            IAtividadeRepository atividadeRepository,
            IMateriaRepository materiaRepository,
            IAuditoriaService auditoriaService)
        {
            _vectorSearch = vectorSearch;
            _llmService = llmService;
            _atividadeRepository = atividadeRepository;
            _materiaRepository = materiaRepository;
            _auditoriaService = auditoriaService;
        }

        public async Task<AtividadeGeradaDto> GerarAtividadeAsync(
            int idMateria, string temaDesejado, TipoAtividade tipoAtividade, int idProfessorLogado)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                throw new InvalidOperationException("Matéria não encontrada");

            var chunksRelevantes = await _vectorSearch.BuscarChunksRelevantesAsync(idMateria, temaDesejado);

            if (chunksRelevantes == null || !chunksRelevantes.Any())
                throw new InvalidOperationException("Nenhuma fonte encontrada para essa matéria e tema");

            var instrucaoAlternativas = tipoAtividade == TipoAtividade.MultiplaEscolha
                ? "\"alternativas\" (um array com EXATAMENTE 4 strings, cada uma sendo uma opção completa, ex.: \"A) Texto da opção\") e "
                : "\"alternativas\" (um array vazio []) e ";

            var prompt = $"Crie uma atividade do tipo '{tipoAtividade}' sobre o tema '{temaDesejado}' " +
                         $"usando os seguintes conteúdos: {string.Join(", ", chunksRelevantes)}. " +
                         "Formate a resposta EXCLUSIVAMENTE como um JSON válido com três propriedades: " +
                         "\"enunciado\" (somente o texto da pergunta, SEM as opções de resposta), " +
                         instrucaoAlternativas +
                         "\"gabarito\" (se for múltipla escolha, apenas a letra correta, como \"A\"; " +
                         "caso contrário, a resposta esperada completa, sem explicações). " +
                         "Não inclua formatação markdown como ```json, retorne apenas o objeto JSON puro.";

            var respostaLlm = await _llmService.GerarRespostaAsync(prompt);
            var jsonLimpo = respostaLlm.Replace("```json", "").Replace("```", "").Trim();

            RespostaIaDto atividadeGerada;
            try
            {
                atividadeGerada = JsonSerializer.Deserialize<RespostaIaDto>(jsonLimpo, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                                  ?? throw new InvalidOperationException("Falha ao desserializar o JSON do LLM.");
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"O LLM não retornou um JSON válido. Resposta original: {respostaLlm}", ex);
            }

            if (tipoAtividade == TipoAtividade.MultiplaEscolha &&
                (atividadeGerada.Alternativas == null || atividadeGerada.Alternativas.Count(a => !string.IsNullOrWhiteSpace(a)) < 2))
            {
                throw new InvalidOperationException(
                    $"O LLM não retornou alternativas suficientes para uma atividade de múltipla escolha. Resposta original: {respostaLlm}");
            }

            var alternativasFinais = atividadeGerada.Alternativas ?? new List<string>();
            var gabaritoFinal = atividadeGerada.Gabarito;

            if (tipoAtividade == TipoAtividade.MultiplaEscolha)
            {
                (alternativasFinais, gabaritoFinal) = EmbaralharAlternativas(atividadeGerada.Alternativas!, atividadeGerada.Gabarito);
            }

            var novaAtividade = new Atividade(
                nome: temaDesejado,
                tipo: tipoAtividade,
                conteudo: atividadeGerada.Enunciado,
                gabarito: gabaritoFinal,
                idMateria: idMateria,
                alternativas: alternativasFinais
            );

            var atividadeCriada = await _atividadeRepository.AdicionarAsync(novaAtividade);

            await _auditoriaService.RegistrarAsync(
                idProfessorLogado, "Atividade.Gerar", "Atividade", atividadeCriada.Id.ToString(), $"Tema: {temaDesejado}");

            return new AtividadeGeradaDto(
                atividadeCriada.Id,
                atividadeCriada.Nome,
                atividadeCriada.Tipo.ToString(),
                atividadeCriada.Conteudo,
                atividadeCriada.Alternativas,
                atividadeCriada.Gabarito,
                atividadeCriada.IdMateria);
        }

        /// <summary>
        /// Embaralha a ordem das alternativas e recalcula a letra do gabarito de acordo com a nova
        /// posição da resposta correta — independe de qualquer tentativa (não confiável) de pedir
        /// variação de posição diretamente à IA.
        /// </summary>
        private static (List<string> Alternativas, string Gabarito) EmbaralharAlternativas(
            List<string> alternativasOriginais, string gabaritoOriginal)
        {
            var textos = alternativasOriginais
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .Select(RemoverPrefixoDeLetra)
                .ToList();

            int indiceCorretoOriginal = LetraParaIndice(gabaritoOriginal);
            string textoCorreto = indiceCorretoOriginal >= 0 && indiceCorretoOriginal < textos.Count
                ? textos[indiceCorretoOriginal]
                : textos[0]; 

            var embaralhadas = textos.OrderBy(_ => Random.Shared.Next()).ToList();

            int novoIndiceCorreto = embaralhadas.IndexOf(textoCorreto);
            if (novoIndiceCorreto < 0)
                novoIndiceCorreto = 0;

            var letras = new[] { "A", "B", "C", "D", "E", "F" };
            var comPrefixo = embaralhadas
                .Select((texto, i) => $"{letras[Math.Min(i, letras.Length - 1)]}) {texto}")
                .ToList();

            return (comPrefixo, letras[Math.Min(novoIndiceCorreto, letras.Length - 1)]);
        }

        private static string RemoverPrefixoDeLetra(string alternativa)
        {
            return Regex.Replace(alternativa.Trim(), @"^[A-Za-z][\)\.\-:]\s*", "");
        }

        private static int LetraParaIndice(string letra)
        {
            if (string.IsNullOrWhiteSpace(letra))
                return -1;

            char c = char.ToUpperInvariant(letra.Trim()[0]);
            return c is >= 'A' and <= 'Z' ? c - 'A' : -1;
        }

        private record RespostaIaDto(string Enunciado, List<string>? Alternativas, string Gabarito);
    }
}