using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record SolicitarRedefinicaoSenhaDto(
        [param: Required(ErrorMessage = "O e-mail é obrigatório.")]
        [param: EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        string Email);
}