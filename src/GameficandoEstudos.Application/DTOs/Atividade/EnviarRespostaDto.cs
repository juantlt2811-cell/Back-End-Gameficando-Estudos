using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record EnviarRespostaDto(
        [param: Required(ErrorMessage = "A resposta não pode ser vazia.")]
        [param: MaxLength(8_000, ErrorMessage = "A resposta deve ter no máximo 8.000 caracteres.")]
        string Resposta);
}