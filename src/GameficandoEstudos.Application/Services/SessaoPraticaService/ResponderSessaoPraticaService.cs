using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Application.Services.SessaoPraticaService
{
    public class ResponderSessaoPraticaService
    {
        private readonly ISessaoPraticaRepository _sessaoPraticaRepository;
        private readonly IAtividadeRepository _atividadeRepository;
        private readonly ILlmService _llmService;

        public ResponderSessaoPraticaService(
            ISessaoPraticaRepository sessaoPraticaRepository,
            IAtividadeRepository atividadeRepository,
            ILlmService llmService)
        {
            _sessaoPraticaRepository = sessaoPraticaRepository;
            _atividadeRepository = atividadeRepository;
            _llmService = llmService;
        }

        public async Task<ResultadoAvaliacaoDto> ExecutarAsync(int idAluno, int idSessao, int idAtividade, string resposta)
        {
            if (string.IsNullOrWhiteSpace(resposta))
                throw new ArgumentException("A resposta não pode ser vazia.", nameof(resposta));

            var sessao = await _sessaoPraticaRepository.ObterPorIdComTentativasAsync(idSessao);
            if (sessao == null || sessao.IdUsuario != idAluno)
                throw new UnauthorizedAccessException("Sessão de prática não encontrada.");

            var atividade = await _atividadeRepository.ObterPorIdAsync(idAtividade);
            if (atividade == null || atividade.IdMateria != sessao.IdMateria)
                throw new InvalidOperationException("Essa questão não pertence a esta sessão de prática.");

            ResultadoAvaliacaoDto resultado;
            if (atividade.Tipo == TipoAtividade.MultiplaEscolha)
            {
                bool acertou = resposta.Trim().Equals(atividade.Gabarito.Trim(), StringComparison.OrdinalIgnoreCase);
                resultado = new ResultadoAvaliacaoDto(acertou ? 100 : 0, acertou ? "Resposta correta!" : "Resposta incorreta.");
            }
            else
            {
                resultado = await _llmService.AvaliarRespostaAsync(atividade.Conteudo, resposta, atividade.Gabarito, atividade.Tipo);
            }

            var tentativa = new TentativaResposta(
                sessao.Id, atividade.Id, idAluno, sessao.IdMateria, resposta, resultado.Pontuacao, resultado.Feedback);

            sessao.RegistrarTentativa(tentativa);
            await _sessaoPraticaRepository.AtualizarAsync(sessao);

            return resultado;
        }
    }
}