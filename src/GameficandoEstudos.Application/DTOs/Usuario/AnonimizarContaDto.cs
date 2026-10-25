using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record AnonimizarContaDto(
        [param: Required(ErrorMessage = "A senha é obrigatória para confirmar a exclusão da conta.")]
        [param: MaxLength(128, ErrorMessage = "A senha deve ter no máximo 128 caracteres.")]
        string SenhaAtual);
}