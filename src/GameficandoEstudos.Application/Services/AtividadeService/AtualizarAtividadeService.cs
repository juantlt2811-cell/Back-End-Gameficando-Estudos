using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class AtualizarAtividadeService
    {
        private readonly IAtividadeRepository _atividadeRepository;
        private readonly IMateriaRepository _materiaRepository;

        public AtualizarAtividadeService(IAtividadeRepository atividadeRepository, IMateriaRepository materiaRepository)
        {
            _atividadeRepository = atividadeRepository;
            _materiaRepository = materiaRepository;
        }

        public async Task ExecutarAsync(AtualizarAtividadeDto dto, int idProfessorLogado)
        {
            var atividade = await _atividadeRepository.ObterPorIdAsync(dto.Id);
            if (atividade == null)
                throw new InvalidOperationException("Atividade não encontrada.");

            var materia = await _materiaRepository.ObterPorIdAsync(atividade.IdMateria);
            if (materia == null || materia.IdProfessor != idProfessorLogado)
                throw new UnauthorizedAccessException("Você não tem permissão para alterar atividades de outros professores.");

            atividade.Atualizar(dto.Nome, dto.Conteudo, dto.Gabarito);

            await _atividadeRepository.AtualizarAsync(atividade);
        }
    }
}