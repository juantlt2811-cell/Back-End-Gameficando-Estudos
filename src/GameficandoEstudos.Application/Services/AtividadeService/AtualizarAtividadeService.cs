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
        private readonly IAuditoriaService _auditoriaService;

        public AtualizarAtividadeService(IAtividadeRepository atividadeRepository, IMateriaRepository materiaRepository, IAuditoriaService auditoriaService)
        {
            _atividadeRepository = atividadeRepository;
            _materiaRepository = materiaRepository;
            _auditoriaService = auditoriaService;
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
            await _auditoriaService.RegistrarAsync(idProfessorLogado, "Atividade.Atualizar", "Atividade", dto.Id.ToString());
        }
    }
}