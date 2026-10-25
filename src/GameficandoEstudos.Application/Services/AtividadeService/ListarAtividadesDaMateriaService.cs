using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Enums;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class ListarAtividadesDaMateriaService
    {
        private readonly IAtividadeRepository _atividadeRepository;
        private readonly IMateriaRepository _materiaRepository;

        public ListarAtividadesDaMateriaService(
            IAtividadeRepository atividadeRepository,
            IMateriaRepository materiaRepository)
        {
            _atividadeRepository = atividadeRepository;
            _materiaRepository = materiaRepository;
        }

        public async Task<IEnumerable<AtividadeDto>> ExecutarAsync(
            int idMateria, TipoUsuario tipoUsuarioLogado, int idUsuarioLogado)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(idMateria);
            if (materia == null)
                return Enumerable.Empty<AtividadeDto>();

            var podeVerInativas =
                tipoUsuarioLogado == TipoUsuario.Admin ||
                (tipoUsuarioLogado == TipoUsuario.Professor && materia.IdProfessor == idUsuarioLogado);

            var atividades = await _atividadeRepository.ObterPorMateriaAsync(idMateria);

            if (!podeVerInativas)
                atividades = atividades.Where(a => a.Ativa);

            return atividades.Select(a => new AtividadeDto(
                a.Id,
                a.Nome,
                a.Tipo.ToString(),
                a.Conteudo,
                a.Alternativas,
                a.IdMateria,
                a.Ativa
            ));
        }
    }
}