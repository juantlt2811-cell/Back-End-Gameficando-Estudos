using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record AnonimizarContaDto(
        [param: Required(ErrorMessage = "A senha é obrigatória para confirmar a exclusão da conta.")]
        string SenhaAtual);
}