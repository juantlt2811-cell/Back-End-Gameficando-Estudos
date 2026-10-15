using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record AtualizarMateriaDto(
        [param: Range(1, int.MaxValue, ErrorMessage = "Id da matéria inválido.")]
        int Id,

        [param: Required(ErrorMessage = "O nome da matéria é obrigatório.")]
        [param: MaxLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
        string Nome,

        [param: Range(1, int.MaxValue, ErrorMessage = "A carga horária deve ser maior que zero.")]
        int CargaHoraria,

        [param: Required(ErrorMessage = "A dificuldade é obrigatória.")]
        [param: MaxLength(50, ErrorMessage = "A dificuldade deve ter no máximo 50 caracteres.")]
        string Dificuldade);
}