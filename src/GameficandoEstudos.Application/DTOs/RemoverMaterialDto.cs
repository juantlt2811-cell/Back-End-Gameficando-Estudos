using System;
using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record RemoverMaterialDto(
        [param: Range(1, int.MaxValue, ErrorMessage = "Id da matéria inválido.")]
        int IdMateria,

        Guid DocumentoId);
}