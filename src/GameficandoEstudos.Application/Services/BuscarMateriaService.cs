using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services
{
    public class BuscarMateriaService
    {
        private readonly IMateriaRepository _materiaRepository;

        public BuscarMateriaService(IMateriaRepository materiaRepository)
        {
            _materiaRepository = materiaRepository;
        }

        public async Task<MateriaDto> ObterPorIdAsync(int id)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(id);

            if (materia == null)
                throw new InvalidOperationException("Matéria não encontrada.");

            return MapearParaDto(materia);
        }

        public async Task<IEnumerable<MateriaDto>> PesquisarAsync(string? nomeMateria = null, int? idProfessor = null)
        {
            var materias = await _materiaRepository.PesquisarAsync(nomeMateria, idProfessor);

            return materias.Select(MapearParaDto);
        }
        public async Task<IEnumerable<MateriaDto>> ObterMateriasDoProfessorAsync(int idProfessor)
        {
            if (idProfessor <= 0)
                throw new ArgumentException("ID do professor inválido.");

            var materias = await _materiaRepository.ObterPorProfessorAsync(idProfessor);

            return materias.Select(MapearParaDto);
        }

        private static MateriaDto MapearParaDto(GameficandoEstudos.Domain.Entities.Materia materia)
        {
            return new MateriaDto(
                materia.Id,
                materia.Nome,
                materia.CargaHoraria,
                materia.Dificuldade,
                materia.IdProfessor,
                materia.CriadoEm
            );
        }
    }
}