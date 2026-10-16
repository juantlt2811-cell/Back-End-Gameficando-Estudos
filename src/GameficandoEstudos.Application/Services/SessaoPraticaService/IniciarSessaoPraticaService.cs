using System.Linq;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Application.Services.SessaoPraticaService
{
    public class IniciarSessaoPraticaService
    {
        private const int QuestoesPorSessao = 5;

        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly IAtividadeRepository _atividadeRepository;
        private readonly ISessaoPraticaRepository _sessaoPraticaRepository;

        public IniciarSessaoPraticaService(
            IUsuarioRepository usuarioRepository,
            IMateriaRepository materiaRepository,
            IAtividadeRepository atividadeRepository,
            ISessaoPraticaRepository sessaoPraticaRepository)
        {
            _usuarioRepository = usuarioRepository;
            _materiaRepository = materiaRepository;
            _atividadeRepository = atividadeRepository;
            _sessaoPraticaRepository = sessaoPraticaRepository;
        }

        public async Task<SessaoPraticaIniciadaDto> ExecutarAsync(int idAluno, int idMateria)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(idAluno);
            if (usuario == null || usuario.Tipo != TipoUsuario.Aluno)
                throw new UnauthorizedAccessException("Apenas alunos podem iniciar sessões de prática.");

            if (!usuario.Gamificacao!.PodeRealizarAtividade())
                throw new InvalidOperationException("Limite diário de matérias praticadas atingido. Volte amanhã!");

            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                throw new InvalidOperationException("Matéria não encontrada.");

            var questoes = await _atividadeRepository.ObterAleatoriasPorMateriaAsync(idMateria, QuestoesPorSessao);
            if (questoes.Count == 0)
                throw new InvalidOperationException("Esta matéria ainda não possui questões disponíveis.");

            var sessao = new SessaoPratica(idAluno, idMateria, questoes.Count);
            await _sessaoPraticaRepository.AdicionarAsync(sessao);

            return new SessaoPraticaIniciadaDto(
                sessao.Id,
                questoes.Select(q => new QuestaoSessaoDto(q.Id, q.Nome, q.Tipo, q.Conteudo, q.Alternativas)).ToList());
        }
    }
}