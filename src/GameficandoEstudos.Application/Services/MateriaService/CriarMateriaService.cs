using GameficandoEstudos.Application.Interfaces;
using GameficandoEstudos.Domain.Entities;
using GameficandoEstudos.Application.DTOs;

namespace GameficandoEstudos.Application.Services.MateriaService
{
    public class CriarMateriaService
    {
        private readonly IMateriaRepository _materiaRepository;
        private readonly IAuditoriaService _auditoriaService;

        public CriarMateriaService(IMateriaRepository materiaRepository, IAuditoriaService auditoriaService)
        {
            _materiaRepository = materiaRepository;
            _auditoriaService = auditoriaService;
        }

        public async Task<int> ExecutarAsync(CriarMateriaDto dto, int idProfessor)
        {
            var materia = new Materia(dto.Nome, dto.CargaHoraria, dto.Dificuldade, idProfessor);
            var materiaCriada = await _materiaRepository.AdicionarAsync(materia);

            await _auditoriaService.RegistrarAsync(idProfessor, "Materia.Criar", "Materia", materiaCriada.Id.ToString());

            return materiaCriada.Id;
        }
    }
}