using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GameficandoEstudos.Application.Services.AtividadeService
{
    public class ListarMateriaisDaMateriaService
    {
        private readonly ICaixaFontesRepository _caixaFontesRepository;
        private readonly IMateriaRepository _materiaRepository;

        public ListarMateriaisDaMateriaService(ICaixaFontesRepository caixaFontesRepository, IMateriaRepository materiaRepository)
        {
            _caixaFontesRepository = caixaFontesRepository;
            _materiaRepository = materiaRepository;
        }

        public async Task<IEnumerable<MaterialBaseConhecimentoDto>> ExecutarAsync(PesquisarMateriaisRequisicaoDto request)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(request.IdMateria);
            if (materia == null || materia.IdProfessor != request.IdProfessorLogado)
                throw new UnauthorizedAccessException("Acesso negado à base de conhecimento desta matéria.");

            var resumos = await _caixaFontesRepository.PesquisarResumoMateriaisAsync(request.IdMateria, request.TermoPesquisa);

            return resumos.Select(r => new MaterialBaseConhecimentoDto(r.DocumentoId, r.Titulo, r.QuantidadeChunks, r.UltimaAtualizacao));
        }
    }
}