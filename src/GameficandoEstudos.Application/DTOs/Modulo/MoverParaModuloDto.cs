using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record MoverParaModuloDto(
        [param: Range(1, int.MaxValue, ErrorMessage = "Id do módulo inválido.")]
        int? IdModulo);
}
