using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record CriarModuloDto(
        [param: Range(1, int.MaxValue, ErrorMessage = "Id da matéria inválido.")]
        int IdMateria,

        [param: Required(ErrorMessage = "O nome do módulo é obrigatório.")]
        [param: MaxLength(100, ErrorMessage = "O nome do módulo deve ter no máximo 100 caracteres.")]
        string Nome);
}
