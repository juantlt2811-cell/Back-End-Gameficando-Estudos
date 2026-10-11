using System;
using System.Threading.Tasks;
using GameficandoEstudos.Application.DTOs;
using GameficandoEstudos.Application.Interfaces;

namespace GameficandoEstudos.Application.Services.MateriaService
{
    public class BuscarMateriaPorIdService
    {
        private readonly IMateriaRepository _materiaRepository;

        public BuscarMateriaPorIdService(IMateriaRepository materiaRepository)
        {
            _materiaRepository = materiaRepository;
        }

        public async Task<MateriaDto> ExecutarAsync(int id, int idUsuario)
        {
            var materia = await _materiaRepository.ObterPorIdAsync(id);

            if (materia == null || materia.IdProfessor != idUsuario)
                throw new UnauthorizedAccessException("Matéria não encontrada ou acesso negado.");

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