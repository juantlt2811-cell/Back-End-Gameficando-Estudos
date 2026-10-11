using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class TestarAtividadeService
    {
        private readonly ILlmService _llmService;
        private readonly IAtividadeRepository _atividadeRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public TestarAtividadeService(
            ILlmService llmService,
            IAtividadeRepository atividadeRepository,
            IMateriaRepository materiaRepository,
            IUsuarioRepository usuarioRepository)
        {
            _llmService = llmService;
            _atividadeRepository = atividadeRepository;
            _materiaRepository = materiaRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<ResultadoAvaliacaoDto> ExecutarAsync(int idProfessorLogado, int idAtividade, string respostaTeste)
        {
            if (string.IsNullOrWhiteSpace(respostaTeste))
                throw new ArgumentException("A resposta de teste não pode ser vazia.", nameof(respostaTeste));

            var usuario = await _usuarioRepository.ObterPorIdAsync(idProfessorLogado);
            if (usuario == null || usuario.Tipo != TipoUsuario.Professor)
                throw new UnauthorizedAccessException("Apenas professores podem acessar a área de testes de atividades.");

            var atividade = await _atividadeRepository.ObterPorIdAsync(idAtividade);
            if (atividade == null)
                throw new InvalidOperationException("Atividade não encontrada.");

            var materia = await _materiaRepository.ObterPorIdAsync(atividade.IdMateria);
            if (materia == null || materia.IdProfessor != idProfessorLogado)
                throw new UnauthorizedAccessException("Você não tem permissão para testar atividades de outras matérias.");

            ResultadoAvaliacaoDto resultadoAvaliacao;

            if (atividade.Tipo == TipoAtividade.MultiplaEscolha)
            {
                bool acertou = respostaTeste.Trim().Equals(atividade.Gabarito.Trim(), StringComparison.OrdinalIgnoreCase);
                string feedback = acertou ? "[MODO TESTE] Resposta correta!" : "[MODO TESTE] Resposta incorreta.";
                resultadoAvaliacao = new ResultadoAvaliacaoDto(acertou, feedback);
            }
            else
            {
                resultadoAvaliacao = await _llmService.AvaliarRespostaAsync(atividade.Conteudo, respostaTeste, atividade.Gabarito);
                resultadoAvaliacao = new ResultadoAvaliacaoDto(resultadoAvaliacao.Aprovado, $"[MODO TESTE] {resultadoAvaliacao.Feedback}");
            }

            return resultadoAvaliacao;
        }
    }
}