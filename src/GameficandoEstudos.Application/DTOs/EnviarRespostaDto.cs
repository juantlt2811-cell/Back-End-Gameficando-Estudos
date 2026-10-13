using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record EnviarRespostaDto(
        [param: Required(ErrorMessage = "A resposta não pode ser vazia.")]
        string Resposta);
}