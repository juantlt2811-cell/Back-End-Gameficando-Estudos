using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Application.DTOs;

namespace GameficandoEstudos.Application.Services.MateriaService
{
    public class CriarMateriaService
    {
        private readonly IMateriaRepository _materiaRepository;

        public CriarMateriaService(IMateriaRepository materiaRepository)
        {
            _materiaRepository = materiaRepository;
        }

        public async Task<int> ExecutarAsync(CriarMateriaDto dto, int idProfessor)
        {
            var materia = new Materia(
                dto.Nome,
                dto.CargaHoraria,
                dto.Dificuldade,
                idProfessor
            );


            var materiaCriada = await _materiaRepository.AdicionarAsync(materia);
            return materiaCriada.Id;
        }
    }
}