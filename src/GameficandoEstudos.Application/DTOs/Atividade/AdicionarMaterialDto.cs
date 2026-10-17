using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record AdicionarMaterialDto(
        [param: Range(1, int.MaxValue, ErrorMessage = "Id da matéria inválido.")]
        int IdMateria,

        [param: Required(ErrorMessage = "O título do material é obrigatório.")]
        [param: MaxLength(200, ErrorMessage = "O título deve ter no máximo 200 caracteres.")]
        string TituloMaterial,

        [param: Required(ErrorMessage = "O texto do material não pode ser vazio.")]
        [param: MaxLength(500_000, ErrorMessage = "O texto do material excede o tamanho máximo permitido (500.000 caracteres).")]
        string TextoCompleto);
}