using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Application.Services.SessaoPraticaService
{
    public class VerificarPodeIniciarSessaoService
    {
        private const int QuestoesPorSessao = 5;

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IAtividadeRepository _atividadeRepository;

        public VerificarPodeIniciarSessaoService(
            IUsuarioRepository usuarioRepository,
            IMateriaRepository materiaRepository,
            IAtividadeRepository atividadeRepository)
        {
            _usuarioRepository = usuarioRepository;
            _materiaRepository = materiaRepository;
            _atividadeRepository = atividadeRepository;
        }

        public async Task<ConfirmacaoInicioSessaoDto> ExecutarAsync(int idAluno, int idMateria)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(idAluno);
            if (usuario == null || usuario.Tipo != TipoUsuario.Aluno)
                throw new UnauthorizedAccessException("Apenas alunos podem praticar matérias.");

            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                throw new InvalidOperationException("Matéria não encontrada.");

            var totalQuestoes = await _atividadeRepository.ContarAtivasPorMateriaAsync(idMateria);
            var tentativasRestantes = usuario.Gamificacao!.TentativasRestantesHoje;

            string? motivo = null;
            if (totalQuestoes == 0)
                motivo = "Esta matéria ainda não possui questões disponíveis.";
            else if (tentativasRestantes == 0)
                motivo = "Limite diário de matérias praticadas atingido. Volte amanhã!";

            return new ConfirmacaoInicioSessaoDto(
                PodeIniciar: motivo == null,
                Motivo: motivo,
                TentativasRestantesHoje: tentativasRestantes,
                TotalQuestoesDisponiveis: Math.Min(totalQuestoes, QuestoesPorSessao));
        }
    }
}