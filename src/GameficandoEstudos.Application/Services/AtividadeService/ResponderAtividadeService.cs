using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;
using System;
using System.Threading.Tasks;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class ResponderAtividadeService
    {
        private readonly ILlmService _llmService;
        private readonly IAtividadeRepository _atividadeRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public ResponderAtividadeService(ILlmService llmService, IAtividadeRepository atividadeRepository, IUsuarioRepository usuarioRepository)
        {
            _llmService = llmService;
            _atividadeRepository = atividadeRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<ResultadoAvaliacaoDto> ResponderAsync(int idUsuario, int idAtividade, string respostaAluno)
        {
            if (string.IsNullOrWhiteSpace(respostaAluno))
                throw new ArgumentException("A resposta do aluno não pode ser vazia.", nameof(respostaAluno));

            var usuario = await _usuarioRepository.ObterPorIdAsync(idUsuario);
            if (usuario == null)
                throw new InvalidOperationException("Usuário não encontrado.");

            if (usuario.Tipo != TipoUsuario.Aluno)
                throw new UnauthorizedAccessException("Apenas alunos podem responder atividades.");

            if (!usuario.Gamificacao!.PodeRealizarAtividade())
                throw new InvalidOperationException("Limite diário de atividades atingido. Volte amanhã!");

            var atividade = await _atividadeRepository.ObterPorIdAsync(idAtividade);
            if (atividade == null)
                throw new InvalidOperationException("Atividade não encontrada.");

            ResultadoAvaliacaoDto resultadoAvaliacao;

            if (atividade.Tipo == TipoAtividade.MultiplaEscolha)
            {
                bool acertou = respostaAluno.Trim().Equals(atividade.Gabarito.Trim(), StringComparison.OrdinalIgnoreCase);
                string feedback = acertou ? "Resposta correta!" : "Resposta incorreta.";
                resultadoAvaliacao = new ResultadoAvaliacaoDto(acertou, feedback);
            }
            else
            {
                resultadoAvaliacao = await _llmService.AvaliarRespostaAsync(atividade.Conteudo, respostaAluno, atividade.Gabarito);
            }

            usuario.Gamificacao.RegistrarAtividadeConcluida();
            await _usuarioRepository.AtualizarAsync(usuario);

            return resultadoAvaliacao;
        }
    }
}