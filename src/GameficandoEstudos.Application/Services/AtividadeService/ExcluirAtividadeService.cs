using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Application.DTOs;


namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class ExcluirAtividadeService
    {
        private readonly IAtividadeRepository _atividadeRepository;
        private readonly IMateriaRepository _materiaRepository; 
        private readonly IAuditoriaService _auditoriaService;

        public ExcluirAtividadeService(IAtividadeRepository atividadeRepository, IMateriaRepository materiaRepository, IAuditoriaService auditoriaService)
        {
            _atividadeRepository = atividadeRepository;
            _materiaRepository = materiaRepository;
            _auditoriaService = auditoriaService;   
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
                await _auditoriaService.RegistrarAsync(idProfessorLogado, "Atividade.Inativar", "Atividade", idAtividade.ToString());
            }
            else
            {
                await _atividadeRepository.RemoverAsync(atividade);
                await _auditoriaService.RegistrarAsync(idProfessorLogado, "Atividade.Remover", "Atividade", idAtividade.ToString());
            }
        }
    }
}