using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record GerarAtividadeDto(
        [param: Range(1, int.MaxValue, ErrorMessage = "Id da matéria inválido.")]
        int IdMateria,

        [param: Required(ErrorMessage = "O tema é obrigatório.")]
        [param: MaxLength(150, ErrorMessage = "O tema deve ter no máximo 150 caracteres.")]
        string Tema,

        [param: Range(1, 3, ErrorMessage = "Tipo de atividade inválido.")]
        int TipoAtividade);
}