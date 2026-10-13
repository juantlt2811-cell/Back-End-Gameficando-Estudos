using System.ComponentModel.DataAnnotations;

namespace GameficandoEstudos.Application.DTOs
{
    public record RedefinirSenhaDto(
        [param: Required(ErrorMessage = "O e-mail é obrigatório.")]
        [param: EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        string Email,

        [param: Required(ErrorMessage = "O token é obrigatório.")]
        string Token,

        [param: Required(ErrorMessage = "A nova senha é obrigatória.")]
        [param: MinLength(8, ErrorMessage = "A senha deve ter no mínimo 8 caracteres.")]
        string NovaSenha);
}