using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.SessaoPraticaService
{
    public class ConcluirSessaoPraticaService
    {
        private readonly ISessaoPraticaRepository _sessaoPraticaRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public ConcluirSessaoPraticaService(
            ISessaoPraticaRepository sessaoPraticaRepository,
            IUsuarioRepository usuarioRepository)
        {
            _sessaoPraticaRepository = sessaoPraticaRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<SessaoPraticaResumoDto> ExecutarAsync(int idAluno, int idSessao)
        {
            var sessao = await _sessaoPraticaRepository.ObterPorIdComTentativasAsync(idSessao);
            if (sessao == null || sessao.IdUsuario != idAluno)
                throw new UnauthorizedAccessException("Sessão de prática não encontrada.");

            var usuario = await _usuarioRepository.ObterPorIdAsync(idAluno);
            if (usuario == null)
                throw new InvalidOperationException("Usuário não encontrado.");

            sessao.Concluir();
            usuario.Gamificacao!.RegistrarAtividadeConcluida();

            await _sessaoPraticaRepository.AtualizarAsync(sessao);
            await _usuarioRepository.AtualizarAsync(usuario);

            return new SessaoPraticaResumoDto(
                sessao.Tentativas.Count, sessao.TotalAcertos, sessao.PercentualAcerto, usuario.Gamificacao.OfensivaAtual);
        }
    }
}