using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record AtualizarAtividadeDto(
        [param: Range(1, int.MaxValue, ErrorMessage = "Id da atividade inválido.")]
        int Id,

        [param: Required(ErrorMessage = "O nome da atividade é obrigatório.")]
        [param: MaxLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        string Nome,

        [param: Required(ErrorMessage = "O conteúdo da atividade é obrigatório.")]
        [param: MaxLength(10_000, ErrorMessage = "O conteúdo deve ter no máximo 10.000 caracteres.")]
        string Conteudo,

        [param: Required(ErrorMessage = "O gabarito da atividade é obrigatório.")]
        [param: MaxLength(10_000, ErrorMessage = "O gabarito deve ter no máximo 10.000 caracteres.")]
        string Gabarito,

        List<string>? Alternativas = null);
}