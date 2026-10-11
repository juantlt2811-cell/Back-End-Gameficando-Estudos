using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class ExcluirAtividadeService
    {
        private readonly IAtividadeRepository _atividadeRepository;
        private readonly IMateriaRepository _materiaRepository; 

        public ExcluirAtividadeService(IAtividadeRepository atividadeRepository, IMateriaRepository materiaRepository)
        {
            _atividadeRepository = atividadeRepository;
            _materiaRepository = materiaRepository;
        }

        public async Task ExecutarAsync(int idAtividade, int idProfessorLogado)
        {
            var atividade = await _atividadeRepository.ObterPorIdAsync(idAtividade);

            if (atividade == null)
                throw new InvalidOperationException("Atividade não encontrada.");

            var materia = await _materiaRepository.ObterPorIdAsync(atividade.IdMateria);
            if (materia == null || materia.IdProfessor != idProfessorLogado)
            {
                throw new UnauthorizedAccessException("Você não tem permissão para alterar ou excluir atividades de outros professores.");
            }

            bool jaFoiRespondida = await _atividadeRepository.PossuiRespostasAsync(idAtividade);

            if (jaFoiRespondida)
            {
                atividade.Inativar();
                await _atividadeRepository.AtualizarAsync(atividade);
            }
            else
            {
                await _atividadeRepository.RemoverAsync(atividade);
            }
        }
    }
}