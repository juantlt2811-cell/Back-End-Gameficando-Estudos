using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Application.Services.UsuarioService
{
    public class ExportarDadosUsuarioService
    {
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IMateriaRepository _materiaRepository;
        private readonly ISessaoPraticaRepository _sessaoPraticaRepository;

        public ExportarDadosUsuarioService(
            IUsuarioRepository usuarioRepository,
            IMateriaRepository materiaRepository,
            ISessaoPraticaRepository sessaoPraticaRepository)
        {
            _usuarioRepository = usuarioRepository;
            _materiaRepository = materiaRepository;
            _sessaoPraticaRepository = sessaoPraticaRepository;
        }

        public async Task<DadosPessoaisExportadosDto> ExecutarAsync(int idUsuario)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(idUsuario);
            if (usuario == null)
                throw new InvalidOperationException("Usuário não encontrado.");

            GamificacaoExportadaDto? gamificacao = usuario.Gamificacao == null
                ? null
                : new GamificacaoExportadaDto(
                    usuario.Gamificacao.OfensivaAtual,
                    usuario.Gamificacao.MaiorOfensiva,
                    usuario.Gamificacao.AtividadesFeitasHoje);

            List<MateriaExportadaDto>? materiasCriadas = null;
            List<SessaoPraticaExportadaDto>? sessoesDePratica = null;

            if (usuario.Tipo == TipoUsuario.Professor)
            {
                var materias = await _materiaRepository.ObterPorProfessorAsync(idUsuario);
                materiasCriadas = materias
                    .Select(m => new MateriaExportadaDto(m.Id, m.Nome, m.CargaHoraria, m.Dificuldade))
                    .ToList();
            }
            else
            {
                var sessoes = await _sessaoPraticaRepository.ListarPorUsuarioAsync(idUsuario);
                sessoesDePratica = sessoes
                    .Select(s => new SessaoPraticaExportadaDto(
                        s.Id, s.IdMateria, s.Status.ToString(), s.TotalAcertos, s.Tentativas.Count,
                        s.IniciadaEm, s.ConcluidaEm))
                    .ToList();
            }

            return new DadosPessoaisExportadosDto(
                usuario.Id, usuario.Nome, usuario.Email, usuario.Tipo.ToString(), usuario.CriadoEm,
                gamificacao, materiasCriadas, sessoesDePratica);
        }
    }
}