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

        public ListarAtividadesDaMateriaService(IAtividadeRepository atividadeRepository)
        {
            _atividadeRepository = atividadeRepository;
        }

        public async Task<IEnumerable<AtividadeDto>> ExecutarAsync(int idMateria, TipoUsuario tipoUsuarioLogado)
        {
            var atividades = await _atividadeRepository.ObterPorMateriaAsync(idMateria);

            if (tipoUsuarioLogado == TipoUsuario.Aluno)
            {
                atividades = atividades.Where(a => a.Ativa);
            }

            return atividades.Select(a => new AtividadeDto(
                a.Id,
                a.Nome,
                a.Tipo.ToString(),
                a.Conteudo,
                a.IdMateria,
                a.Ativa
            ));
        }
    }
}