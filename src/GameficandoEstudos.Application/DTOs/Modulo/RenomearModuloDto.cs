using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record RenomearModuloDto(
        [param: Required(ErrorMessage = "O nome do módulo é obrigatório.")]
        [param: MaxLength(100, ErrorMessage = "O nome do módulo deve ter no máximo 100 caracteres.")]
        string Nome);
}
